// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by appliZiple law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.OneGet.Builtin {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Implementation;
    using Utility.Collections;
    using Utility.Deployment.Compression;
    using Utility.Deployment.Compression.Zip;
    using Utility.Extensions;
    using Utility.Plugin;

    public class ZipArchiver  {
        public IEnumerable<string> SupportedFormats {
            get {
                return new[] {
                    "zip"
                };
            }
        }

        public void InitializeProvider(object dynamicInterface, object requestImpl) {
        }

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns></returns>
        public string GetArchiverName() {
            return "zip";
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object requestImpl) {
            return _UnpackArchive(localFilename, destinationFolder, requestImpl).ByRefEnumerable();
        }

        private IEnumerable<string> _UnpackArchive(string localFilename, string destinationFolder, Object requestImpl) {


            var info = new ZipInfo(localFilename);
            var files = info.GetFiles();
            var percent = 0;
            var index = 0;
            var processed = new List<string>();

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Unpacking {0} {1}", localFilename, destinationFolder);
                try {
                    info.Unpack(destinationFolder, (sender, args) => {
                        if (args.ProgressType == ArchiveProgressType.FinishFile) {
                            processed.Add( Path.Combine(destinationFolder, args.CurrentFileName));
                            index++;
                            var complete = (index*100)/files.Count;
                            if (complete != percent) {
                                percent = complete;
                                request.Debug("Percent Complete {0}", percent);
                            }
                            /*
                         * Does not currently support cancellation . 
                         * Todo: add cancellation support to DTF compression classes.
                         * */
                            if (request.IsCancelled()) {
                                throw new OperationCanceledException("cancelling");
                            }
                        }
                    });
                }
                catch (OperationCanceledException) {
                    // no worries.
                }
                catch (Exception e) {
                    e.Dump();
                }

                request.Debug("DONE Unpacking {0} {1}", localFilename, destinationFolder);
            }
            // return the list of files to the parent.
            return processed.ToArray();

#if manually            
            var result = new CancellableBlockingCollection<string>();
            Task.Factory.StartNew(() => {
                using (var request = requestImpl.As<Request>()) {
                    request.Debug("Unpacking {0} {1}", localFilename, destinationFolder);

                    var info = new ZipInfo(localFilename);
                    var files = info.GetFiles();
                    request.Debug("File Count", files.Count);

                    var percent = 0;
                    var index = 0;

                    foreach (var zipfile in info) {
                        index ++;
                        var complete = (index*100)/files.Count;
                        if (complete != percent) {
                            percent = complete;
                            request.Debug("Percent Complete {0}", percent);
                        }
                        var outputFilename = Path.Combine(destinationFolder, zipfile.Path, zipfile.Name);
                        zipfile.CopyTo(outputFilename, true);
                        result.Add(outputFilename);
                    }
                    request.Debug("DONE Unpacking {0} {1}", localFilename, destinationFolder);
                    result.CompleteAdding();
                }

            });
                return CancellableBlockingCollection<string>.ToCancellableEnumerable(result);
#endif
            /*
            var info = new ZipInfo(localFilename);
            
            foreach (var zipfile in info.GetFiles()) {
                var outputFilename = Path.Combine(destinationFolder, zipfile.Path, zipfile.Name);
                zipfile.CopyTo(outputFilename, true);

                yield return outputFilename;
            }
             * */
        }

        public bool IsSupportedArchive(string localFilename) {
            try {
                var ze = new ZipEngine();
                using (var zipFile = File.OpenRead(localFilename)) {
                    return ze.IsArchive(zipFile);
                }
            } catch {

            }
            return false;
        }
    }
}