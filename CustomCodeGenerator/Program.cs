// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace CustomCodeGenerator {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;


    internal static class Extensions {
        public static string GetValue(this Match m, string name) {
            return m.Groups[name].Value;
        }

        public static IEnumerable<Match> FindIn(this Regex rx, string text) {
            return rx.Matches(text).Cast<Match>();
        }

        public static TSource SafeAggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) {
            if (source != null && source.Any()) {
                return source.Aggregate(func);
            }
            return default(TSource);
        }


        public static string Combined(this IEnumerable<string> strings ) {
            return strings.SafeAggregate((current, each) => current + "\r\n" + each);
        }
        public static string format(this string formatString, params object[] args) {
            if (args == null || args.Length == 0) {
                return formatString;
            }

            try {
                // first, try to replace 
                formatString = new Regex(@"\$\{(?<macro>\w*?)\}").Replace(formatString, new MatchEvaluator((m) => {
                    var key = m.Groups["macro"].Value;
                    int v = 0;
                    if (int.TryParse(key, out v)) {
                        return "${" + key + "}";
                    }
                    var p = args[0].GetType().GetProperty(key);
                    if (p != null) {
                        return p.GetValue(args[0]).ToString();
                    }

                    return "${{" + key+ "}}";
                }));
                return String.Format(CultureInfo.CurrentCulture, formatString, args);
            }
            catch (Exception) {
                return formatString.Replace('{', '[').Replace('}', ']');
            }
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
            foreach (var each in collection) {
                action(each);
            }
        }

        public static string TrimWhitespace(this string text) {
            return string.IsNullOrEmpty(text) ? text : text.Trim('\t', ' ', '\r', '\n');
        }
    }
    internal class Program {

        private static int Usage() {
            Console.Error.WriteLine("\r\nUsage:\r\n----------");
            Console.Error.WriteLine("CustomCodeGenerator <solutiondirectory> <targetdirectory1> [[targetdirectory2] [targetdirectory3]...]");
            return 1;
        }
        private static int Main(string[] args) {
            if (!args.Any()) {
                Console.Error.WriteLine("No directories given.");
                return Usage();
            }

            if (args.Length < 2) {
                Console.Error.WriteLine("No target directories given.");
                return Usage();
            }

            try {
                return new Program(args[0], args.Skip(1)).Run();
            } catch (Exception e) {
                Console.Error.WriteLine("{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace);
                return 2;
            }
       }

        private Regex RegionRx(string type, string suffix) {
            return new Regex(string.Format(@"(?<whitespace>[\x20\t]*)#region\s*{0}\s*(?<name>\w*?)-{1}\s*(?<content>.*?)#endregion", type, suffix), RegexOptions.Singleline);
        }

        private IEnumerable<Match> GetRegion(string type, string suffix, string text) {
            return RegionRx(type, suffix).FindIn(text);
        }

        private string ReplaceRegion(string type, string suffix, string text, Func<string,string,string,string> replaceFunc ) {
            return RegionRx(type, suffix).Replace(text, new MatchEvaluator((arg) =>
                "{3}#region {0} {1}-{2}\r\n{4}\r\n{3}#endregion\r\n".format(
                    type, 
                    arg.GetValue("name"), 
                    suffix, 
                    arg.GetValue("whitespace"), 
                    replaceFunc(arg.GetValue("name"), arg.GetValue("content"), arg.GetValue("whitespace"))
                )));

        }

        private string ModifyRegion(string type, string suffix, string text, Func<string, string, string, string> replaceFunc) {
            return RegionRx(type, suffix).Replace(text, new MatchEvaluator((arg) =>
                    replaceFunc(arg.GetValue("name"), arg.GetValue("content"), arg.GetValue("whitespace"))
                ));

        }

        private static string InsertDashInCamelCaseString(string txt) {
            var first = txt.IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXY".ToCharArray());
            var second = 0;
            do {
                second = txt.IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXY".ToCharArray(), first + 1);
                if (second < 0) {
                    return txt;
                }
                if (second == first + 1) {
                    first = second;
                    continue;
                }
                return txt.Insert( second,"-");
            } while (true);
        }

        private static string GetPsDelegateName(string txt) {
            return  InsertDashInCamelCaseString(txt).Trim('s');
        }


        /*
        private string ScanForReturnsAttribute(string preamble, string returntype) {
            var returnsRx = new Regex(@"\s*\[Returns\(typeof\((?<type>[\w,\<\>]+)\)\).*?\]");
            return returnsRx.FindIn(preamble).Select(each => each.GetValue("type")).FirstOrDefault(each => !string.IsNullOrEmpty(each)) ?? returntype;
        }
        private string ScanForYieldWithAttribute(string preamble, string returntype) {
            var returnsRx = new Regex(@"\s*\[.*?YieldWith\(typeof\((?<type>[\w,\<\>]+)\)\).*?\]");
            return returnsRx.FindIn(preamble).Select(each => each.GetValue("type")).FirstOrDefault(each => !string.IsNullOrEmpty(each)) ?? returntype;
        }
        */

        private string RemoveReturnsAttribute(string preamble) {
            var returnsRx = new Regex(@"\s*\[Returns.*?\]\s*");
            return returnsRx.Replace(preamble,"");
        }

        private int Run() {
            var sourceFiles = Directory.EnumerateFiles(_solutionDir, "*.cs", SearchOption.AllDirectories).Where( each => each.IndexOf(@"\intermediate\" ) == -1 && each.IndexOf(@"\output\" ) == -1 &&each.IndexOf(@"\CustomCodeGenerator\" ) == -1 );
            
            var contents = sourceFiles.Select(File.ReadAllText).ToArray();

            var parameterRx = new Regex(@"\s*(?<type>[\w,\<\>]+)\s*(?<name>\w+)\s*(?<init>=\s*[\w\.]*\s*)?\s*?(?:\,)?");
            // var delegateRx = new Regex(@"\s*(?<preamble>.*?)(?<scope>public|internal)\s*delegate\s*(?<TRet>\S*)\s*(?<name>\w+)\((?<params>.*?)\).*?;", RegexOptions.Singleline);
            var interfaceRx = new Regex(@"\s*(?<preamble>.*?)\s*(?<TRet>\S*)\s*(?<name>\w+)\((?<params>.*?)\).*?;", RegexOptions.Singleline);

            // ------------------------------------------------------------------------------------------------------------------------------
            // scan for #region declare *-apis
            // var apiRx = new Regex(@"#region\s*declare\s*(?<name>.*?)-apis\s*(?<content>.*?)#endregion", RegexOptions.Singleline);
            // var apiRx = RegionRx("declare", "apis");
            // var apis = contents.SelectMany(text => apiRx.Matches(text).Cast<Match>().Select(match => new {
            // var apis = contents.SelectMany(text => RegionRx("declare", "apis").Matches(text).Cast<Match>().Select(match => new {

            // ------------------------------------------------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------------------------------------------------
            var apis = contents.SelectMany(text => GetRegion("declare","apis",text).Select(match => new {
                Name = match.GetValue("name"),
                Content = match.GetValue("content"),
                WhiteSpace = match.GetValue("whitespace")
            })).ToArray();

            var apiDeclarations = apis.SelectMany(region => interfaceRx.FindIn(region.Content).Select(match => new {
                category = region.Name,
                whiteSpace = region.WhiteSpace,
                preamble = RemoveReturnsAttribute(match.GetValue("preamble").TrimWhitespace()),
                rawpreamble = match.GetValue("preamble").TrimWhitespace(),
                returnType = match.GetValue("TRet"),
                // abstractReturnType = ScanForReturnsAttribute(match.GetValue("preamble").TrimWhitespace(), match.GetValue("TRet")),
                // yieldWithType = ScanForYieldWithAttribute(match.GetValue("preamble").TrimWhitespace(),""),
                delegateName = match.GetValue("name"),
                psDelegateName = GetPsDelegateName( match.GetValue("name")),
                // scope = match.GetValue("scope"),
                parameterText = match.GetValue("params") ,
                isVoid = match.GetValue("TRet") == "void",
                returnKeyword = match.GetValue("TRet") == "void" ? "" : "return ",
                defaultResult = match.GetValue("TRet") == "void" ? "{{ }}" : "default({0})".format(match.GetValue("TRet")),
                generatedResult = match.GetValue("TRet") == "void" ? "" : "default({0});".format(match.GetValue("TRet")),
                parameterCount = parameterRx.FindIn(match.GetValue("params")).Count(),
                comma = parameterRx.FindIn(match.GetValue("params")).Any() ? "," : "",
                parameterNames = parameterRx.FindIn(match.GetValue("params")).Select(p => p.GetValue("name")).SafeAggregate( (current,each) => current + ","+each )??"",
                fixedParameterNames = parameterRx.FindIn(match.GetValue("params")).Select(p => "p" + p.GetValue("name")).SafeAggregate((current, each) => current + "," + each) ?? "",
                parameterTypes = parameterRx.FindIn(match.GetValue("params")).Select(p => p.GetValue("type")).SafeAggregate((current, each) => current + "," + each)??"",
                customParameterText = parameterRx.FindIn(match.GetValue("params")).Select(p => {
                    var t = p.GetValue("type");
                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    if (t == "IEnumerable<object>" && n == "args") {
                        return "params object[] args";
                    }
                    return "{0} {1} {2}".format(t,n,i);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",
                psParameterText = parameterRx.FindIn(match.GetValue("params")).Where( each => each.GetValue("type") != "Callback" ).Select(p => {
                    var t = p.GetValue("type");
                    if (t.Equals("IEnumerable<string>")) {
                        t = "string[]";
                    }
                    if (t.Equals("IEnumerable<object>")) {
                        t = "object[]";
                    }

                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    return "\r\n        [{0}] ${1}".format(t, n);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",

                abstractParameterText = parameterRx.FindIn(match.GetValue("params")).Select(p => {
                    var t = p.GetValue("type");
                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    if (t == "IEnumerable<object>" && n == "args") {
                        return "params object[] args";
                    }
                    if (t == "Callback") {
                        return "Request request";
                    }
                    return "{0} {1} {2}".format(t, n, i);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",

                parameters = parameterRx.FindIn(match.GetValue("params")).Select(p => new {
                    type = p.GetValue("type"),
                    name = p.GetValue("name"),
                    init = p.GetValue("init"),
                }).ToArray()
            })).ToArray();


            // ------------------------------------------------------------------------------------------------------------------------------
            // scan for #region declare *-interface

            // var interfaceRx = new Regex(@"#region\s*declare\s*(?<name>.*?)-interface\s*(?<content>.*?)#endregion", RegexOptions.Singleline);

            var interfaces = contents.SelectMany(text => GetRegion("declare", "interface", text).Select(match => new {
                Name = match.GetValue("name"),
                Content = match.GetValue("content"),
                WhiteSpace = match.GetValue("whitespace")
            })).ToArray();

            var interfaceDeclarations = interfaces.SelectMany(region => interfaceRx.FindIn(region.Content).Select(match => new {
                category = region.Name,
                whiteSpace = region.WhiteSpace,
                preamble = RemoveReturnsAttribute(match.GetValue("preamble").TrimWhitespace()),
                rawpreamble = match.GetValue("preamble").TrimWhitespace(),
                returnType = match.GetValue("TRet"),
                // abstractReturnType = ScanForReturnsAttribute( match.GetValue("preamble").TrimWhitespace(), match.GetValue("TRet")),
                // yieldWithType = ScanForYieldWithAttribute(match.GetValue("preamble").TrimWhitespace(), ""),
                delegateName = match.GetValue("name"),
                psDelegateName = GetPsDelegateName(match.GetValue("name")),
                // scope = match.GetValue("scope"),
                parameterText = match.GetValue("params"),
                isVoid = match.GetValue("TRet") == "void",
                returnKeyword = match.GetValue("TRet") == "void" ? "" : "return ",
                defaultResult = match.GetValue("TRet") == "void" ? "{{ }}" : "default({0})".format(match.GetValue("TRet")),
                generatedResult = match.GetValue("TRet") == "void" ? "" : "default({0});".format(match.GetValue("TRet")),
                parameterCount = parameterRx.FindIn(match.GetValue("params")).Count(),
                comma = parameterRx.FindIn(match.GetValue("params")).Any() ? "," : "",
                parameterNames = parameterRx.FindIn(match.GetValue("params")).Select(p => p.GetValue("name")).SafeAggregate((current, each) => current + "," + each) ?? "",
                fixedParameterNames = parameterRx.FindIn(match.GetValue("params")).Select(p => "p"+p.GetValue("name")).SafeAggregate((current, each) => current + "," + each) ?? "",
                parameterTypes = parameterRx.FindIn(match.GetValue("params")).Select(p => p.GetValue("type")).SafeAggregate((current, each) => current + "," + each) ?? "",
                IsRequired = match.GetValue("preamble").IndexOf("<required/>") > -1 ,
                customParameterText = parameterRx.FindIn(match.GetValue("params")).Select(p => {
                    var t = p.GetValue("type");
                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    if (t == "IEnumerable<object>" && n == "args") {
                        return "params object[] args";
                    }
                    return "{0} {1} {2}".format(t, n, i);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",

                abstractParameterText = parameterRx.FindIn(match.GetValue("params")).Select(p => {
                    var t = p.GetValue("type");
                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    if (t == "IEnumerable<object>" && n == "args") {
                        return "params object[] args";
                    }
                    if (t == "Callback") {
                        return "Request request";
                    }
                    return "{0} {1} {2}".format(t, n, i);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",


                psParameterText = parameterRx.FindIn(match.GetValue("params")).Where(each => each.GetValue("type") != "Callback").Select(p => {
                    var t = p.GetValue("type");
                    var n = p.GetValue("name");
                    var i = p.GetValue("init");
                    if (t.Equals("IEnumerable<string>")) {
                        t = "string[]";
                    }
                    if (t.Equals("IEnumerable<object>")) {
                        t = "object[]";
                    }

                    return "\r\n        [{0}] ${1}".format(t, n);
                }).SafeAggregate((current, each) => current + "," + each) ?? "",
                parameters = parameterRx.FindIn(match.GetValue("params")).Select(p => new {
                    type = p.GetValue("type"),
                    name = p.GetValue("name"),
                    init = p.GetValue("init"),
                }).ToArray()
            })).ToArray();


            var types = contents.SelectMany(text => GetRegion("declare", "types", text).Select(match => new {
                Name = match.GetValue("name"),
                Content = match.GetValue("content"),
                WhiteSpace = match.GetValue("whitespace")
            })).ToArray();

            // ------------------------------------------------------------------------------------------------------------------------------
            // process code generation regions in files

            

            foreach (var targetDir in _targetDirs) {
                if (!Directory.Exists(targetDir)) {
                    throw new Exception(string.Format("Target dir {0} does not exist", targetDir));
                }

                // for c# files:
                var targetFiles = Directory.EnumerateFiles(targetDir , "*.cs", SearchOption.AllDirectories).Where( each => each.IndexOf(@"\intermediate\" ) == -1 && each.IndexOf(@"\output\" ) == -1 &&each.IndexOf(@"\CustomCodeGenerator\" ) == -1 );
                targetFiles = targetFiles.Union(Directory.EnumerateFiles(targetDir, "*.psm1", SearchOption.AllDirectories).Where(each => each.IndexOf(@"\intermediate\") == -1 && each.IndexOf(@"\output\") == -1 && each.IndexOf(@"\CustomCodeGenerator\") == -1));
                foreach (var targetFile in targetFiles) {
                    var originalText = File.ReadAllText(targetFile);
                    var text = originalText;


#if NOT_USED
                    // generate-resolved *-apis =============================================================================================
                    text = ReplaceRegion("generate-resolved", "apis", text, (name, content,whitespace)=> {
                        return apiDeclarations.Where(each => each.category == name).Select(api => @"
{1}${preamble}
{1}[System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Performance"", ""CA1811:AvoidUncalledPrivateCode"", Justification = ""Generated Code"")]
{1}public static ${returnType} ${delegateName} (this Callback c ${comma} ${parameterText} ) {{
{1}    ${returnKeyword}(c.Resolve<${delegateName}>() ?? ((${fixedParameterNames})=>${defaultResult} ) )(${parameterNames});
{1}}}
".format(api, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each) ;
                    });


                    // generate-dispatcher *-apis =============================================================================================
                    text = ReplaceRegion("generate-dispatcher", "apis", text, (name, content, whitespace) => {
                        return apiDeclarations.Where(each => each.category == name).Select(api => @"
{1}private ${delegateName} _${delegateName};
{1}${preamble}
{1}[System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Performance"", ""CA1811:AvoidUncalledPrivateCode"", Justification = ""Generated Code"")]
{1}public ${returnType} ${delegateName}(${customParameterText} ) {{
{1}    CheckDisposed();
{1}    ${returnKeyword} (_${delegateName} ?? (_${delegateName} = (_callback.Resolve<${delegateName}>() ?? ((${fixedParameterNames})=> ${defaultResult} ) )))(${parameterNames});
{1}}}
".format(api, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });

                    // dispose-dispatcher *-apis =============================================================================================
                    text = ReplaceRegion("dispose-dispatcher", "apis", text, (name, content, whitespace) => {
                        return apiDeclarations.Where(each => each.category == name).Select(api => @"{1}_${delegateName} = null;".format(api, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });

#endif 

                    // implement *-apis =============================================================================================
                    text = ReplaceRegion("implement", "apis", text, (name, content, whitespace) => {
                        var newContent = content;

                        apiDeclarations.Where(each => each.category == name).ForEach(api => {
                            var rxFunc = new Regex(@"\[Implementation\]\s*public\s*(?<TRet>\S*)\s(?<name>"+api.delegateName+@")\s*\((?<params>.*?)\)(?<code>.*)", RegexOptions.Singleline);
                            if (rxFunc.Match(content).Success) {
                                newContent = rxFunc.Replace(newContent, new MatchEvaluator(me => {
                                    var fn = new {
                                        returnType = me.GetValue("TRet"),
                                        fnName = me.GetValue("name"),
                                        currentParameterText = me.GetValue("params"),
                                        parameterText = api.parameterText,
                                        code = me.GetValue("code"),
                                        preamble = api.preamble
                                    };

                                    return @"[Implementation]
{1}public ${returnType} ${fnName}(${parameterText}){2}".format(fn, whitespace,fn.code);
                                }));
                            } else {
                                newContent += @"{1}[Implementation]
{1}public ${returnType} ${delegateName}(${parameterText}){{
{1} // TODO: Fill in implementation
{1}}}".format(api, whitespace);
                            }
                       });


                        return  newContent ;
                    });


                    // copy *-apis =============================================================================================
                    text = ReplaceRegion("copy", "apis", text, (name, content, whitespace) => {
                        return  apiDeclarations.Where(each => each.category == name).Select(api => @"
{1}${preamble}
{1}public abstract ${returnType} ${delegateName}(${parameterText});".format(api, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });


                    // implement *-interface =============================================================================================
                    text = ReplaceRegion("implement", "interface", text, (name, content, whitespace) => {
                        var newContent = content;

                        interfaceDeclarations.Where(each => each.category == name).ForEach(api => {
                            var rxFunc = new Regex(@"\s*public\s*(?<TRet>\S*)\s(?<name>" + api.delegateName + @")\s*\((?<params>.*?)\)(?<code>.*)", RegexOptions.Singleline);
                            if (rxFunc.Match(content).Success) {
                                newContent = rxFunc.Replace(newContent, new MatchEvaluator(me => {
                                    var fn = new {
                                        returnType = me.GetValue("TRet"),
                                        fnName = me.GetValue("name"),
                                        currentParameterText = me.GetValue("params"),
                                        parameterText = api.parameterText,
                                        code = me.GetValue("code"),
                                        preamble = api.preamble
                                    };

                                    return @"
{1}public ${returnType} ${fnName}(${parameterText}){2}".format(fn, whitespace, fn.code);
                                }));
                            }
                            else {
                                if (api.parameterTypes.IndexOf("Callback") != -1) {
                                    newContent += @"{1}${preamble}
{1}public ${returnType} ${delegateName}(${parameterText}){{
    {1} // TODO: Fill in implementation
    {1} // Delete this method if you do not need to implement it
    {1} // Please don't throw an not implemented exception, it's not optimal.
    {1}using (var request = Request.New(c)) {{
    {1}    // use the request object to interact with the OneGet core:
    {1}    request.Debug(""Information"",""Calling '${delegateName}'"" );
    {1}}}

    {1}${returnKeyword} ${generatedResult}
{1}}}

".format(api, whitespace);
                                } else {
                                    newContent += @"{1}${preamble}
{1}public ${returnType} ${delegateName}(${parameterText}){{
    {1} // TODO: Fill in implementation
    {1} // Delete this method if you do not need to implement it
    {1} // Please don't throw an not implemented exception, it's not optimal.

    {1}${returnKeyword} ${generatedResult}
{1}}}

".format(api, whitespace);
                                }
                            }
                        });


                        return newContent;
                    });

                    // implement-ps *-interface =============================================================================================
                    text = ReplaceRegion("psimplement", "interface", text, (name, content, whitespace) => {
                        var newContent = content;

                        interfaceDeclarations.Where(each => each.category == name).ForEach(api => {
                            var rxFunc = new Regex(@"\s*function\s*(?<name>" + api.psDelegateName + @")\s*\{\s*param\((?<params>.*?)\)(?<code>.*)", RegexOptions.Singleline);
                            if (rxFunc.Match(content).Success) {
                                newContent = rxFunc.Replace(newContent, new MatchEvaluator(me => {
                                    var fn = new {
                                        // returnType = me.GetValue("TRet"),
                                        fnName = api.psDelegateName,
                                        currentParameterText = me.GetValue("params"),
                                        parameterText = api.parameterText,
                                        psParameterText = api.psParameterText,
                                        code = me.GetValue("code"),
                                        preamble = api.preamble
                                    };

                                    return @"
function ${fnName} {{ 
    param(${psParameterText}
    ){1}".format(fn, fn.code); // use the positional parameter, since fn.code has braces in it.
                                }));
                            }
                            else {
                                if (api.parameterTypes.IndexOf("Callback") != -1) {
                                    newContent += @"<# 
${preamble}
#>
function ${psDelegateName} {{
    param(${psParameterText}
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it
    
    # use the request object to interact with the OneGet core:
    $request.Debug(""Information"",""Calling '${delegateName}'"" );
   
    # expected return type : ${returnType}
    # ${returnKeyword} $null;
}}

".format(api);
                                }
                                else {
                                    newContent += @"<# 
${preamble}
#>
function ${psDelegateName} {{
    param(${psParameterText}
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : ${returnType}
    # ${returnKeyword} $null;
}}

".format(api);
                                }
                            }
                        });


                        return newContent ;
                    });

                    // psgenerate-resolved *-interface =============================================================================================
                    text = ReplaceRegion("generate-pswrapper", "apis", text, (name, content, whitespace) => {
                        return apiDeclarations.Where(each => each.category == name).Select(api => @"<# 
${preamble}
#>
function ${psDelegateName} {{
    param(${psParameterText}
    )
}}".format(api, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });



                    // abstract *-interface =============================================================================================
                    text = ReplaceRegion("abstract", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Select(fn => @"
{1}${preamble}
{1}public abstract ${abstractReturnType} ${delegateName}(${abstractParameterText});
".format(fn, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });

                    // abstract *-interface =============================================================================================
                    text = ReplaceRegion("abstract", "apis", text, (name, content, whitespace) => {
                        return apiDeclarations.Where(each => each.category == name).Select(fn => @"
{1}${preamble}
{1}public abstract ${abstractReturnType} ${delegateName}(${abstractParameterText});
".format(fn, whitespace)).SafeAggregate((current, each) => current + "\r\n" + each);
                    });



                    text = ReplaceRegion("generate-memberinit", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Select(fn => @"{1}${delegateName} = Get{2}Delegate<Interface.${delegateName}>(instance);".format(fn, whitespace,fn.IsRequired ? "Required" : "Optional")).Combined();
                    });

                    text = ReplaceRegion("generate-members", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Select(fn => @"{1}internal readonly Interface.${delegateName} ${delegateName};".format(fn, whitespace)).Combined();
                    });

                    text = ReplaceRegion("generate-enum", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Select(fn => @"{1}${delegateName},".format(fn, whitespace)).Combined();
                    });

                    /*
                    text = ReplaceRegion("generate-issupported", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Select(fn => @"{1} case {2}Api.${delegateName}:
{1}    return _provider.${delegateName}.IsSupported();".format(fn, whitespace,name)).Combined();
                    });
                    */

                    
                    /*
                    text = ReplaceRegion("generate-istypecompatible", "interface", text, (name, content, whitespace) => {
                        return interfaceDeclarations.Where(each => each.category == name).Where( fn => fn.IsRequired ).Select(fn => @"{1}    && publicMethods.Any(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, ""${delegateName}"") && typeof (Interface.${delegateName}).IsDelegateAssignableFromMethod(each))".format(fn, whitespace, name)).Combined();
                    });
                    */


                    // copy *-types =============================================================================================
                    text = ReplaceRegion("copy", "types", text, (name, content, whitespace) => {
                        return types.Where(each => each.Name == name).Select(t => t.Content).SafeAggregate((current, each) => current + "\r\n" + each);
                    });


                    //-------------------------------------------------------------------------------------------------------------------------
                    //-------------------------------------------------------------------------------------------------------------------------
                    // if we have any changes, let's make sure we clean it up =================================================================
                    if (originalText != text) {
                        // couple of last minute fixes.
                        // remove excessive blank lines
                        text = new Regex("^\\s*$", RegexOptions.Multiline).Replace(text, "\r\n");
                        text = text.Replace("\r\n", "«");
                        text = text.Replace("\n", "«");
                        text = text.Replace("\r", "«");
                        text = text.Replace("«««", "««");

                        text = text.Replace("«", "\r\n");
                        text = text.TrimWhitespace();
                    }


                    // Save the file if it actually changed =============================================================================================
                    if (originalText != text) {
                        Console.WriteLine("Updating : {0}", targetFile);

                        // rename original file to originalfile.ext.###.bak 
                        var fileDir = Path.GetDirectoryName(targetFile);
                        var filename = Path.GetFileName(targetFile);

                        var count = Directory.EnumerateFiles(fileDir, filename + ".*.bak").Count();
                        var backup = Path.Combine(fileDir, string.Format("{0}.{1}.bak", filename, count));

                        File.Move(targetFile,backup );
                        if (File.Exists(targetFile)) {
                            throw new Exception(string.Format("File '{0}' didn't move to '{1}'", targetFile, backup));
                        }

                        File.WriteAllText(targetFile, text);
                    }
                }

            }

            return 0;
        }

        private string _solutionDir;
        private string[] _targetDirs;

        Program(string solution, IEnumerable<string> targets) {
            _solutionDir = Path.GetFullPath(solution);
            if (!Directory.Exists(_solutionDir)) {
                throw new Exception("Solution Directory does not exist.");
            }

            _targetDirs = targets.Select(Path.GetFullPath).ToArray();
        }
    }
}