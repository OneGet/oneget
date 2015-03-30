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

namespace Microsoft.PackageManagement.Packaging {
    using System.Collections;
    using System.Text.RegularExpressions;

    public class MediaQuery {
#if MEDIA_QUERY_DOCUMENTATION
         An expression that the document evaluator can use to determine if the
        target of the link is applicable to the current platform (the host
        environment)

        Used as an optimization hint to notify a system that it can
        ignore something when it's not likely to be used.

        The format of this string is modeled upon the MediaQuery definition at
        http://www.w3.org/TR/css3-mediaqueries/

        This is one or more EXPRESSIONs where the items are connected
        with an OPERATOR:

          media="EXPRESSION [[OPERATOR] [EXPRESSION]...]"

        EXPRESSION is processed case-insensitive and defined either :
          (ENVIRONMENT)
            indicates the presence of the environment
        or
          ([PREFIX-]ENVIRONMENT.ATTRIBUTE:VALUE)
            indicates a comparison of an attribute of the environment.

        ENVIRONMENT is a text identifier that specifies any software,hardware
          feature or aspect of the system the software is intended to run in.

          Common ENVIRONMENTs include (but not limited to):
            linux
            windows
            java
            powershell
            ios
            chipset
            peripheral

        ATTRIBUTE is a property of an ENVIRONMENT with a specific value.
          Common attributes include (but not limited to):
            version
            vendor
            architecture

        PREFIX is defined as one of:
          MIN    # property has a minimum value of VALUE
          MAX    # property has a maximum value of VALUE

          if a PREFIX is not provided, then the property should equal VALUE

        OPERATOR is defined of one of:
          AND
          NOT

        Examples:
          media="(windows)"
              // applies to only systems that identify themselves as 'Windows'

          media="(windows) not (windows.architecture:x64)"
              // applies to only systems that identify
              // themselves as windows and are not for an x64 cpu

          media="(windows) and (min-windows.version:6.1)"
              // applies to systems that identify themselves as
              // windows and at least version 6.1

          media="(linux) and (linux.vendor:redhat) and (min-linux.kernelversion:3.0)"
              // applies to systems that identify themselves as
              // linux, made by redhat and with a kernel version of at least 3.0

          media="(freebsd) and (min-freebsd.kernelversion:6.6)"
              // applies to systems that identify themselves as
              // freebsd, with a kernel version of at least 6.6

          media="(powershell) and (min-powershell.version:3.0)"
              // applies to systems that have powershell 3.0 or greater

        Properties are expected to be able to be resolved by the host
        environment without having to do significant computation.


        example strings:
            "windows"
            "(windows)"
            "((windows))"
            "((windows) and (microsoft))"


        // a single expression should be either
            NAME
            min-NAME:value


        EXPR: (<EXPR>) OP (<EXPR>)

        tokenizer:
        (?<open>\()|(?<close>\))|(?<operator>not)|(?<operator>and)|(?<txt>[\w\:\-\.]+)
#endif

        private static readonly Regex _expressionRegex = new Regex(@"[\w\-\.]");

        private static readonly Regex _queryRegex = new Regex("");

        public static bool IsApplicable(string mediaQuery, Hashtable environment) {
            // todo: implement applicability
            return true;
        }
    }
}