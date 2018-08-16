//---------------------------------------------------------------------
// <copyright file="Entities.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.WindowsInstaller.Linq
{
    // Silence warnings about style and doc-comments
#if !CODE_ANALYSIS
#pragma warning disable 1591

    #region Generated code

    internal class Component_ : QRecord
    {
        public string Component { get => this[0]; set => this[0] = value; }
        public string ComponentId { get => this[1]; set => this[1] = value; }
        public string Directory_ { get => this[2]; set => this[2] = value; }
        public string Condition { get => this[4]; set => this[4] = value; }
        public string KeyPath { get => this[5]; set => this[5] = value; }

        public ComponentAttributes Attributes
        { get => (ComponentAttributes)I(3); set => this[3] = ((int)value).ToString(); }
    }

    internal class CreateFolder_ : QRecord
    {
        public string Directory_ { get => this[0]; set => this[0] = value; }
        public string Component_ { get => this[1]; set => this[1] = value; }
    }

    internal class CustomAction_ : QRecord
    {
        public string Action { get => this[0]; set => this[0] = value; }
        public string Source { get => this[2]; set => this[2] = value; }
        public string Target { get => this[3]; set => this[3] = value; }

        public CustomActionTypes Type
        { get => (CustomActionTypes)I(1); set => this[1] = ((int)value).ToString(); }
    }

    internal class Directory_ : QRecord
    {
        public string Directory { get => this[0]; set => this[0] = value; }
        public string Directory_Parent { get => this[1]; set => this[1] = value; }
        public string DefaultDir { get => this[2]; set => this[2] = value; }
    }

    internal class DuplicateFile_ : QRecord
    {
        public string FileKey { get => this[0]; set => this[0] = value; }
        public string Component_ { get => this[1]; set => this[1] = value; }
        public string File_ { get => this[2]; set => this[2] = value; }
        public string DestName { get => this[4]; set => this[4] = value; }
        public string DestFolder { get => this[5]; set => this[5] = value; }
    }

    internal class Feature_ : QRecord
    {
        public string Feature { get => this[0]; set => this[0] = value; }
        public string Feature_Parent { get => this[1]; set => this[1] = value; }
        public string Title { get => this[2]; set => this[2] = value; }
        public string Description { get => this[3]; set => this[3] = value; }
        public int? Display { get => NI(4); set => this[4] = value.ToString(); }
        public int Level { get => I(5); set => this[5] = value.ToString(); }
        public string Directory_ { get => this[6]; set => this[6] = value; }

        public FeatureAttributes Attributes
        { get => (FeatureAttributes)I(7); set => this[7] = ((int)value).ToString(); }
    }

    [DatabaseTable("FeatureComponents")]
    internal class FeatureComponent_ : QRecord
    {
        public string Feature_ { get => this[0]; set => this[0] = value; }
        public string Component_ { get => this[1]; set => this[1] = value; }
    }

    internal class File_ : QRecord
    {
        public string File { get => this[0]; set => this[0] = value; }
        public string Component_ { get => this[1]; set => this[1] = value; }
        public string FileName { get => this[2]; set => this[2] = value; }
        public int FileSize { get => I(3); set => this[3] = value.ToString(); }
        public string Version { get => this[4]; set => this[4] = value; }
        public string Language { get => this[5]; set => this[5] = value; }
        public int Sequence { get => I(7); set => this[7] = value.ToString(); }

        public FileAttributes Attributes
        { get => (FileAttributes)I(6); set => this[6] = ((int)value).ToString(); }
    }

    [DatabaseTable("MsiFileHash")]
    internal class FileHash_ : QRecord
    {
        public string File_ { get => this[0]; set => this[0] = value; }
        public int Options { get => I(1); set => this[1] = value.ToString(); }
        public int HashPart1 { get => I(2); set => this[2] = value.ToString(); }
        public int HashPart2 { get => I(3); set => this[3] = value.ToString(); }
        public int HashPart3 { get => I(4); set => this[4] = value.ToString(); }
        public int HashPart4 { get => I(5); set => this[5] = value.ToString(); }
    }

    [DatabaseTable("InstallExecuteSequence")]
    internal class InstallSequence_ : QRecord
    {
        public string Action { get => this[0]; set => this[0] = value; }
        public string Condition { get => this[1]; set => this[1] = value; }
        public int Sequence { get => I(2); set => this[2] = value.ToString(); }
    }

    internal class LaunchCondition_ : QRecord
    {
        public string Condition { get => this[0]; set => this[0] = value; }
        public string Description { get => this[1]; set => this[1] = value; }
    }

    internal class Media_ : QRecord
    {
        public int DiskId { get => I(0); set => this[0] = value.ToString(); }
        public int LastSequence { get => I(1); set => this[1] = value.ToString(); }
        public string DiskPrompt { get => this[2]; set => this[2] = value; }
        public string Cabinet { get => this[3]; set => this[3] = value; }
        public string VolumeLabel { get => this[4]; set => this[4] = value; }
        public string Source { get => this[5]; set => this[5] = value; }
    }

    internal class Property_ : QRecord
    {
        public string Property { get => this[0]; set => this[0] = value; }
        public string Value { get => this[1]; set => this[1] = value; }
    }

    internal class Registry_ : QRecord
    {
        public string Registry { get => this[0]; set => this[0] = value; }
        public string Key { get => this[2]; set => this[2] = value; }
        public string Name { get => this[3]; set => this[3] = value; }
        public string Value { get => this[4]; set => this[4] = value; }
        public string Component_ { get => this[5]; set => this[5] = value; }

        public RegistryRoot Root
        { get => (RegistryRoot)I(1); set => this[0] = ((int)value).ToString(); }
    }

    internal class RemoveFile_ : QRecord
    {
        public string FileKey { get => this[0]; set => this[0] = value; }
        public string Component_ { get => this[2]; set => this[2] = value; }
        public string FileName { get => this[3]; set => this[3] = value; }
        public string DirProperty { get => this[4]; set => this[4] = value; }

        public RemoveFileModes InstallMode
        { get => (RemoveFileModes)I(5); set => this[5] = ((int)value).ToString(); }
    }

    #endregion Generated code

#pragma warning restore 1591
#endif // !CODE_ANALYSIS
}