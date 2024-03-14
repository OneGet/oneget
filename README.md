# Introduction

TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project.

# Getting Started

TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:

1. Installation process
2. Software dependencies
3. Latest releases
4. API references

# Central Feed Services (CFS) - Engineering Systems Standard Requirement

CFS onboarding required configuring your project to only consume packages through Azure Artifacts. This is an engineering system standard which is required company-wide.

1. If your project uses NuGet packages, update the nuget.config file placed at the root of this reposiroty to use your preferred feed.
2. If your project uses npm packages, consult [this section of the CFS documentation](https://aka.ms/cfs). Feel free to delete the nuget.config file in this repository.
3. If your project uses Maven packages, consult [this section of the CFS documentation](https://aka.ms/cfs). Feel free to delete the nuget.config file in this repository.
4. If your project uses Pip packages, consult [this section of the CFS documentation](https://aka.ms/cfs). Feel free to delete the nuget.config file in this repository.
5. If your project uses Rust (Cargo) crates, consult [this section of the CFS documentation](https://aka.ms/cfs). Feel free to delete the nuget.config file in this repository.

# Build and Test

TODO: Describe and show how to build your code and run the tests.

# Contribute

TODO: Explain how other users and developers can contribute to make your code better.

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:

-   [ASP.NET Core](https://github.com/aspnet/Home)
-   [Visual Studio Code](https://github.com/Microsoft/vscode)
-   [Chakra Core](https://github.com/Microsoft/ChakraCore)
