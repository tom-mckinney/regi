# Regi

> [ rej-ee ]

[![nuget](https://img.shields.io/nuget/v/Regi)](https://www.nuget.org/packages/Regi/) ![Publish Package](https://github.com/tom-mckinney/regi/workflows/Publish%20Package/badge.svg) ![Run Tests](https://github.com/tom-mckinney/regi/workflows/Run%20Tests/badge.svg?branch=main)

Regi is a "config-first" microservice orchestrator. The first iteration of this project was created to address the growing complexity of a microservice domain without adding vendor dependencies or cumbersome overhead. This project is still a work in progress, and is likely evolve over its lifetime; however, we are committed to always uphold the following standards:

- **Cross platform support**: Regi is actively used on Windows, Mac, and Linux. We feel very strongly that the development of microservice architecture should be accessible to anyone, no matter their choice of platform or technology.
- **Painless configuration**: Our goal is to make your life easier&mdash;you shouldn't have to toil with config files or bash scripts! With features such as auto-configuration and route injection, you can focus more on building software and less on how to integrate it.
- **Open source licensing**: Regi is, and will always be, a free and open source project. Because of this, we are highly receptive to community feedback and contributions. If there's a feature that you'd like added, please submit an [issue](https://github.com/tom-mckinney/regi/issues) or a [pull request](https://github.com/tom-mckinney/regi/pulls).

## Installation

### NuGet

```powershell
dotnet tool install -g regi
```

### Building from source code

Windows:

```powershell
.\build.cmd
```

Mac/Linux:

```powershell
./build.sh
```

## Configuration

Create a file named `regi.json` in a top-level directory with the following command:

```powershell
regi init
```

Regi uses a discovery service in conjunction with a project identifier framework to automate the generation of the `regi.json` configuration file. At this time, the identifiers are shipped as part of the `Regi.Core` package; however, we plan to externalize this so that it is easy to add new identifiers or create your own.

## Framework support

Regi currently supports the following frameworks:

- .NET Core
- NodeJS

There is ongoing work to add native support for a full suite of popular frameworks&mdash;please search for issues with the [framework label](https://github.com/tom-mckinney/regi/issues?q=is%3Aissue+label%3Aframework) to check the current state of this initiative. Additionally, we plan to offer an externalization API that could enable 3rd party "plugins" or allow frameworks to be configured within the `regi.json` file. If you have any ideas or strong feelings about the design of this API, please [join the discussion](https://github.com/tom-mckinney/regi/issues/37) and share your thoughts!

## Usage

<!-- Regi is most commonly used as a .NET Core Global tool (i.e. a command line application). For a detailed look at all of the commands and options offered, please refer to the [wiki documentation](TODO). The most common commands used in daily development are listed below: -->
Regi is most commonly used as a .NET Core Global tool (i.e. a command line application). Detailed documentation is currently a work in progress, so please use the `regi --help` command for the most up-to-date documentation. The most common commands used in daily development are listed below:

- `regi start [project]`

    This command will start every project or any projects that matches the `project` regular expression. It will also start any projects or services that these projects depend on.

- `regi test [project]`

    This command will test every project or any projects that matches the `project` regular expression. If a project has any dependencies, it will start those before executing the tests. Tests are configured to run in parallel by default; however, any project can be configured to run serially by setting the `serial` property to `true`.

- `regi kill [project]`

    This command will kill the processes for every project or any projects that matches the `project` regular expression. By default, Regi will track and kill the processes for every project on shutdown. However, nothing is ever perfect (especially regarding process IO) so this command is quite useful to cleanup any orphaned processes. **Note:** this command will kill all processes that match a given frameworks criteria. Because of this, it will also kill a `dotnet.exe` process that was started without Regi.

## Samples

The most up-to-date examples can be found in the [Sample Projects](https://github.com/tom-mckinney/regi/tree/main/src/Regi.Test/_SampleProjects_) of the `Regi.Test` project. Below is a simple example of what a `regi.json` configuration file might look like:

```json
{
  "apps": [
    {
      "name": "frontend",
      "path": "./Frontend/",
      "framework": "node",
      "port": 3000,
      "commands": {
        "start": "run dev"
      }
    },
    {
      "name": "backend",
      "path": "./Backend/",
      "framework": "dotnet",
      "port": 5000,
      "arguments": {
        "*": [ "--foo bar" ]
      }
    }
  ],
  "tests": [
    {
      "name": "frontend-tests",
      "path": "./Frontend/",
      "framework": "dotnet",
      "type": "unit"
    },
    {
      "name": "backend-tests",
      "path": "./Backend/",
      "framework": "node",
      "type": "unit"
    },
    {
      "name": "integration-tests",
      "path": "./Frontend/",
      "framework": "node",
      "type": "integration",
      "serial": true,
      "commands": {
        "test": "run integration"
      }
    }
  ],
  "services": []
}
```

**Note:** The simplest way to generate one of these files is to run the `regi init` command at the top-level directory of your repository.

## Contribution Instructions

```powershell
dotnet pack --output ./
```

```powershell
dotnet tool uninstall -g regi
```

```powershell
dotnet tool install -g regi --add-source ./
```

```powershell
dotnet nuget push
```