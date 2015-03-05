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

namespace Microsoft.OneGet.Test.Core.Packaging {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using OneGet.Packaging;
    using OneGet.Utility.Extensions;
    using Support;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public class SwidTagTests : Tests {
        public SwidTagTests(ITestOutputHelper outputHelper)
            : base(outputHelper) {
        }

     
      
        [Fact]
        public void SimpleTag() {
            using (CaptureConsole) {

                var swid = new SoftwareIdentity() {
                    Name = "SamplePackage",
                    FullPath = "c:\\tmp\\path",
                    FastPackageReference = "some-string",
                    IsPatch = false,
                    Source = "test-source",
                    SearchKey = "search-key",
                    Status = "test-status",
                    TagId = "some-tag-id",
                    Version = "1.0",
                    VersionScheme = "multipart-numeric",
                    FromTrustedSource = true,
                    IsSupplemental = false,
                    IsCorpus = false,
                    Summary = "Summary Text",
                    AppliesToMedia = "Windows",
                    PackageFilename = "c:\\tmp\\path\\filename.txt",
                    TagVersion = "1",
                };

                // add some arbitrary metadata
                var meta = swid.AddMetadataValue(swid.FastPackageReference, "sample", "value");
                Assert.NotNull(meta);


                var xml = XDocument.Parse(swid.SwidTagText);

                Console.WriteLine("SWID: {0} ", swid.SwidTagText);


                Assert.Equal("SamplePackage", xml.XPathToAttribute("/swid:SoftwareIdentity/@name").Value );

                var v = xml.XPathToAttribute("/swid:SoftwareIdentity/@tagId").Value;

                Assert.Equal("some-tag-id", xml.XPathToAttribute("/swid:SoftwareIdentity/@tagId").Value );
                Assert.Equal("1.0", xml.XPathToAttribute("/swid:SoftwareIdentity/@version").Value );
                Assert.Equal("multipart-numeric", xml.XPathToAttribute("/swid:SoftwareIdentity/@versionScheme").Value );
                Assert.Equal("1", xml.XPathToAttribute("/swid:SoftwareIdentity/@tagVersion").Value );
                Assert.Equal("Windows", xml.XPathToAttribute("/swid:SoftwareIdentity/@media").Value );

                Assert.True(xml.XPathToAttribute("/swid:SoftwareIdentity/@patch").Value.IsFalse() );
                Assert.True(xml.XPathToAttribute("/swid:SoftwareIdentity/@supplemental").Value.IsFalse());
                Assert.True(xml.XPathToAttribute("/swid:SoftwareIdentity/@corpus").Value.IsFalse());

                Assert.Equal("Summary Text",xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Meta/@summary").Value);
              

                Assert.Equal("value", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Meta/@sample").Value);

            }
        }


        [Fact]
        public void EmptyTag() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // validate that an empty tag isn't broken (although, aruguably not entirely valid)
                Assert.Null(swid.Name);
                Assert.Null(swid.IsCorpus); 
                Assert.Null(swid.IsPatch);
                Assert.Null(swid.CanonicalId);
                
                Assert.Null(swid.IsSupplemental);
                Assert.Null(swid.PackageFilename);
                Assert.Null(swid.Provider);
                Assert.Null(swid.ProviderName);
                Assert.Null(swid.SearchKey);
                Assert.Null(swid.Source);
                Assert.Null(swid.Status);
                Assert.Null(swid.Summary);
                
                Assert.Null(swid.FullPath);
                Assert.Null(swid.TagId);
                Assert.Null(swid.TagVersion);
                Assert.Null(swid.Version);
                Assert.Null(swid.VersionScheme);

                Assert.False(swid.FromTrustedSource);

                Assert.Empty(swid.Meta);
                Assert.Empty(swid.Links);
                Assert.Empty(swid.Entities);
                Assert.Null(swid.Payload);
                Assert.Null(swid.Evidence);
            }
        }

        [Fact]
        public void EmptyPayloadAndEvidence() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data
                swid.AddPayload();
                swid.AddEvidence();

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 
                Assert.Empty(swid.Payload.Files);
                Assert.Empty(swid.Payload.Directories);
                Assert.Empty(swid.Payload.Processes);
                Assert.Empty(swid.Payload.Resources);

                Assert.Empty(swid.Evidence.Files);
                Assert.Empty(swid.Evidence.Directories);
                Assert.Empty(swid.Evidence.Processes);
                Assert.Empty(swid.Evidence.Resources);

                Assert.Empty(swid.Payload.Attributes.Values);
                Assert.Empty(swid.Evidence.Attributes.Keys);

                Assert.Null( swid.Evidence.Date );
                Assert.Null(swid.Evidence.DeviceId);
            }
        }

        [Fact]
        public void EntitiesTag() {
            using (CaptureConsole) {

                var swid = new SoftwareIdentity() {
                    Name = "SamplePackage",
                    FastPackageReference = "some-string",
                    Source = "test-source",
                    SearchKey = "search-key",
                    Status = "test-status",
                    TagId = "some-tag-id",
                    Version = "1.0",
                    VersionScheme = "multipart-numeric",
                    TagVersion = "1",
                };

                // add some data
                swid.AddEntity("garrett", "http://fearthecowboy.com/", "author");
                
                var entity = swid.AddEntity("bob", "http://bob.com/", "contributor");
                entity.AddRole("consultant");

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                Assert.Equal("garrett", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Entity[1]/@name").Value);
                Assert.Equal("http://fearthecowboy.com/", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Entity[1]/@regId").Value);
                Assert.Equal("author", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Entity[1]/@role").Value);

                Assert.Equal(2, entity.Roles.Count());
                Assert.Contains("contributor", entity.Roles );
                Assert.Contains("consultant", entity.Roles);

                Assert.Equal("contributor consultant", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Entity[2]/@role").Value);
            }
        }

        [Fact]
        public void WhatHappensWithDuplicateMetadata() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data
                var meta = swid.AddMetadataValue(swid.FastPackageReference, "sample", "value");
                Assert.NotNull(meta);

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 
            }
        }

        [Fact]
        public void Payload() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data
                var payload = swid.AddPayload();

                var dir1 = payload.AddDirectory("dir1");
                dir1.Location = "myapp";
                dir1.Root = "PROGRAMFILES";
                dir1.IsKey = false;

                var dir2 = payload.AddDirectory("dir2");
                var nested1 =dir2.AddDirectory("nested1");
                var file1 = nested1.AddFile("file1");
                file1.Size = 12345;
                file1.Version = "1.0";
                file1.IsKey = true;

                var file2 = payload.AddFile("file2");

                var file3 = payload.AddFile("file3");
                file3.Location = "dir1";
                file3.Root = "SYSTEMDRIVE";

                var process = swid.Payload.AddProcess("foo.exe");
                process.AddAttribute("commandline", "--daemon");


                var regkey = swid.Payload.AddResource("regkey");
                regkey.AddAttribute("key", "hklm/foo/bar/bin/baz");
                regkey.AddAttribute("value", "chocolate");

                var payload2 = swid.AddPayload();
                    
                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 
                // verify only one element is actually created.
                Assert.Equal(payload.ElementUniqueId, payload2.ElementUniqueId);

                Assert.Equal("dir1", dir1.Name);
                Assert.Equal("myapp", dir1.Location);
                Assert.Equal("PROGRAMFILES", dir1.Root );
                Assert.False(dir1.IsKey);

                Assert.Empty( dir1.Files);
                Assert.Empty( dir1.Directories);
             
                Assert.Equal(1,dir2.Directories.Count());
                Assert.Empty(dir2.Files);
                Assert.Equal(1, dir2.Directories.FirstOrDefault().Files.Count());
                Assert.Equal("file1", dir2.Directories.FirstOrDefault().Files.FirstOrDefault().Name );
                Assert.Equal(12345, dir2.Directories.FirstOrDefault().Files.FirstOrDefault().Size);

                Assert.Equal("1.0", dir2.Directories.FirstOrDefault().Files.FirstOrDefault().Version);
                Assert.True(dir2.Directories.FirstOrDefault().Files.FirstOrDefault().IsKey);

                Assert.Equal( 2, swid.Payload.Files.Count());
                Assert.Equal( 1, swid.Payload.Processes.Count() );
                Assert.Equal(1, swid.Payload.Resources.Count());

                Assert.Equal("foo.exe", swid.Payload.Processes.FirstOrDefault().Name);
                Assert.Equal("--daemon", swid.Payload.Processes.FirstOrDefault().GetAttribute("commandline"));

                Assert.Null(swid.Payload.Processes.FirstOrDefault().GetAttribute("notpresent"));

                Assert.Equal("regkey", swid.Payload.Resources.FirstOrDefault().Type);
                Assert.Equal("hklm/foo/bar/bin/baz", swid.Payload.Resources.FirstOrDefault().GetAttribute("key"));
                Assert.Equal("chocolate", swid.Payload.Resources.FirstOrDefault().GetAttribute("value"));

                // try via indexing accessor
                Assert.Equal("chocolate", swid.Payload.Resources.FirstOrDefault().Attributes["value"]);

                // 
                Assert.Equal(3, swid.Payload.Resources.FirstOrDefault().Attributes.Count);
                Assert.Contains("type", swid.Payload.Resources.FirstOrDefault().Attributes.Keys);
                Assert.Contains("key", swid.Payload.Resources.FirstOrDefault().Attributes.Keys);
                Assert.Contains("value", swid.Payload.Resources.FirstOrDefault().Attributes.Keys);

                Assert.DoesNotContain("not-present", swid.Payload.Resources.FirstOrDefault().Attributes.Keys);

                // some xml-based assertions:
                Assert.Equal("dir1",xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Payload/swid:Directory[1]/@name").Value);
                Assert.Equal("dir2", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Payload/swid:Directory[2]/@name").Value);
                Assert.Equal("nested1", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Payload/swid:Directory[2]/swid:Directory[1]/@name").Value);

                Assert.Equal("file1", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Payload/swid:Directory[2]/swid:Directory[1]/swid:File[1]/@name").Value);
                Assert.Equal("12345", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Payload/swid:Directory[2]/swid:Directory[1]/swid:File[1]/@size").Value);

                Assert.Equal(1, xml.XPathToElements("/swid:SoftwareIdentity/swid:Payload").Count());
                Assert.Equal(1, xml.XPathToElements("//swid:Payload").Count());

                Assert.Equal(3, xml.XPathToElements("//swid:File").Count());
                Assert.Equal(1, xml.XPathToElements("//swid:Process").Count());
                Assert.Equal(1, xml.XPathToElements("//swid:Resource").Count());
                Assert.Equal(3, xml.XPathToElements("//swid:Directory").Count());
            }
        }

        [Fact]
        public void Evidence() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data
                var evidence = swid.AddEvidence();

                var now = DateTime.Now;
                evidence.Date = now;

                evidence.DeviceId = "someid";

                var dir1 = evidence.AddDirectory("dir1");
                dir1.Location = "myapp";
                dir1.Root = "PROGRAMFILES";
                dir1.IsKey = false;

                

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 

                // verify only one element is actually created.
                Assert.Equal(evidence.ElementUniqueId, swid.AddEvidence().ElementUniqueId);

                // most is the same code as Payload... 

                // check to make sure the object we got back is the same as we put in.
                dir1 = evidence.Directories.FirstOrDefault();
                Assert.Equal("dir1", dir1.Name);
                Assert.Equal("myapp", dir1.Location);
                Assert.Equal("PROGRAMFILES", dir1.Root);
                Assert.False(dir1.IsKey);

                Assert.Equal(now.ToUniversalTime(),((DateTime)evidence.Date).ToUniversalTime());
                Assert.Equal("someid" ,evidence.DeviceId );

                // some xml validations:
                Assert.Equal("dir1", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Evidence/swid:Directory[1]/@name").Value);

                // check for the device id
                Assert.Equal("someid", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Evidence/@deviceId").Value);
                
                // validate the format of the date
                Assert.Equal(now.ToUniversalTime().ToString("o"), xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Evidence/@date").Value);
            }
        }

        [Fact]
        public void Links() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data
                var link = swid.AddLink( new Uri( "http://foo.com"), "homepage");

                var link2 = swid.AddLink(new Uri("swid:/somepackage-v1.0"), "dependency");
                link2.Artifact = "somepkg";
                link2.Media = "Windows";
                link2.MediaType = "text/none";
                link2.Ownership = "abandon";
                link2.Use = "required";

                var link3 = swid.AddLink(new Uri("http://foo.com/somepackage.msi"), "package");
                link3.Artifact = "somepkg";
                link3.Media = "Windows";
                link3.MediaType = "binary/package";
                link3.Use = "required";

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 
                Assert.Equal(3, swid.Links.Count());
                Assert.Equal("http://foo.com/", link.HRef.ToString());
                Assert.Equal("homepage", link.Relationship);
                Assert.Equal( 2, link.Attributes.Count);
                
                Assert.Equal( link3.Artifact , link2.Artifact);

                // some xml-based assertions:
                Assert.Equal("http://foo.com/", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[1]//@href").Value);

                Assert.Equal("swid:/somepackage-v1.0", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@href").Value);
                Assert.Equal("somepkg", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@artifact").Value);
                Assert.Equal("Windows", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@media").Value);
                Assert.Equal("text/none", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@type").Value);
                Assert.Equal("abandon", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@ownership").Value);
                Assert.Equal("required", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Link[2]/@use").Value);
            }

        }

        [Fact]
        public void Metadata() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                swid.AddAttribute(XNamespace.Get("http://oneget.org/swidtag") + "test", "value");

                // add some data
                var meta1 = swid.AddMeta();
                meta1.AddAttribute("Key", "Value");
                meta1.AddAttribute("Key2", "Value");
                meta1.AddAttribute(XNamespace.Get("http://oneget.org/swidtag") + "other", "somevalue");

                var meta2 = swid.AddMeta();
                meta2.AddAttribute(XNamespace.Get("http://oneget.org/swidtag") + "other2", "somevalue2");
                
                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 
                Assert.Equal( 3, meta1.Attributes.Keys.Count());
                Assert.Equal(1, meta2.Attributes.Keys.Count());

                Assert.Equal("Value", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Meta[1]/@Key").Value);
                Assert.Equal("Value", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Meta[1]/@Key2").Value);
                Assert.Equal("somevalue", xml.XPathToAttribute("/swid:SoftwareIdentity/swid:Meta[1]/@oneget:other").Value);
                
            }

        }

/*
        [Fact]
        public void othertest() {
            using (CaptureConsole) {
                var swid = new SoftwareIdentity();

                // add some data

                // to xml
                var xml = XDocument.Parse(swid.SwidTagText);
                Console.WriteLine("SWID: {0} ", swid.SwidTagText);

                // assertions 

            }

        }
*/
        

    }
}