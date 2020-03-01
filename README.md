# Regi

> [ rej-ee ]

[![nuget](https://img.shields.io/nuget/v/Regi)](https://www.nuget.org/packages/Regi/) ![Publish Package](https://github.com/tom-mckinney/regi/workflows/Publish%20Package/badge.svg) ![Run Tests](https://github.com/tom-mckinney/regi/workflows/Run%20Tests/badge.svg?branch=master)

Regi is a "config-first" microservice orchestrator. The first iteration of this project was created to address the growing complexity of a microservice domain without adding vendor dependencies or cumbersome overhead. This project is still a work in progress, and is likely evolve over its lifetime; however, we are committed to always uphold the following standards:

- **Cross platform support**: Regi is activily used on Windows, Mac, and Linux. We feel very strongly that the development of microservice architecture should be accessible to anyone, no matter their choice of platform or technology.
- **Painless configuration**: Our goal is to make your life easier&mdash;you shouldn't have to toil with config files or bash scripts! With features such as auto-configuration and route injection, you can focus more on building software and less on how to integrate it.
- **Open source licensing**: Regi is, and will always be, a free and open source project. Because of this, we are highly receptive to community feedback and contributions. If there's a feature that you'd like added, please submit an [issue](https://github.com/tom-mckinney/regi/issues) or a [pull request](https://github.com/tom-mckinney/regi/pull-requests).

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

There is ongoing work to add native support for a full suite of popular frameworks&mdash;please search for issues with the `Framework` label to check the current state of this initiative. Additionally, we plan to offer an externalization API that could enable 3rd party framework plugins or allow frameworks to be configured within the `regi.json` file. If you have any ideas or strong feeling about the design of this API, please [join the discussion](TODO) and share your thoughts!

Schema:

```json
{
  "definitions": {},
  "$schema": "",
  "$id": "http://example.com/root.json",
  "type": "object",
  "title": "Regi Configuration Schema",
  "default": null,
  "required": [
    "apps",
    "tests"
  ],
  "properties": {
    "apps": {
      "$id": "#/properties/apps",
      "type": "array",
      "title": "Apps Schema",
      "items": {
        "$id": "#/properties/apps/items",
        "type": "object",
        "title": "Project Schema",
        "default": null,
        "required": [
          "name",
          "path",
          "framework",
          "port"
        ],
        "properties": {
          "name": {
            "$id": "#/properties/name",
            "type": "string",
            "title": "The Name Schema",
            "default": "",
            "examples": [
              "Frontend"
            ],
            "pattern": "^(.*)$"
          },
          "path": {
            "$id": "#/properties/path",
            "type": "string",
            "title": "The Path Schema",
            "default": "",
            "examples": [
              "./Frontend/package.json"
            ],
            "pattern": "^(.*)$"
          },
          "framework": {
            "$id": "#/properties/framework",
            "type": "string",
            "title": "The Framework Schema",
            "default": "",
            "examples": [
              "node"
            ],
            "pattern": "^(.*)$"
          },
          "port": {
            "$id": "#/properties/port",
            "type": "integer",
            "title": "The Port Schema",
            "default": 0,
            "examples": [
              3000
            ]
          },
          "commands": {
            "$id": "#/properties/commands",
            "type": "object",
            "title": "The Commands Schema"
          },
          "serial": {
            "$id": "#/properties/serial",
            "type": "boolean",
            "title": "The Serial Schema",
            "default": false,
            "examples": [
              true
            ]
          },
          "rawOutput": {
            "$id": "#/properties/rawOutput",
            "type": "boolean",
            "title": "The Rawoutput Schema",
            "default": false,
            "examples": [
              true
            ]
          },
          "source": {
            "$id": "#/properties/source",
            "type": "string",
            "title": "The Source Schema",
            "default": "",
            "examples": [
              "https://npmjs.org/api"
            ],
            "pattern": "^(.*)$"
          },
          "requires": {
            "$id": "#/properties/requires",
            "type": "array",
            "title": "The Requires Schema",
            "items": {
              "$id": "#/properties/requires/items",
              "type": "string",
              "title": "Project Name Schema",
              "description": "Reference to another project by name",
              "default": "",
              "examples": [
                "Frontend",
                "Backend"
              ],
              "pattern": "^(.*)$"
            }
          }
        }
      }
    },
    "tests": {
      "$id": "#/properties/tests",
      "type": "array",
      "title": "The Tests Schema",
      "items": {
        "$id": "#/properties/tests/items",
        "type": "object",
        "title": "Project Schema",
        "default": null,
        "required": [
          "name",
          "path",
          "framework",
          "port"
        ],
        "properties": {
          "name": {
            "$id": "#/properties/name",
            "type": "string",
            "title": "The Name Schema",
            "default": "",
            "examples": [
              "Frontend"
            ],
            "pattern": "^(.*)$"
          },
          "path": {
            "$id": "#/properties/path",
            "type": "string",
            "title": "The Path Schema",
            "default": "",
            "examples": [
              "./Frontend/package.json"
            ],
            "pattern": "^(.*)$"
          },
          "framework": {
            "$id": "#/properties/framework",
            "type": "string",
            "title": "The Framework Schema",
            "default": "",
            "examples": [
              "node"
            ],
            "pattern": "^(.*)$"
          },
          "port": {
            "$id": "#/properties/port",
            "type": "integer",
            "title": "The Port Schema",
            "default": 0,
            "examples": [
              3000
            ]
          },
          "commands": {
            "$id": "#/properties/commands",
            "type": "object",
            "title": "The Commands Schema"
          },
          "serial": {
            "$id": "#/properties/serial",
            "type": "boolean",
            "title": "The Serial Schema",
            "default": false,
            "examples": [
              true
            ]
          },
          "rawOutput": {
            "$id": "#/properties/rawOutput",
            "type": "boolean",
            "title": "The Rawoutput Schema",
            "default": false,
            "examples": [
              true
            ]
          },
          "source": {
            "$id": "#/properties/source",
            "type": "string",
            "title": "The Source Schema",
            "default": "",
            "examples": [
              "https://npmjs.org/api"
            ],
            "pattern": "^(.*)$"
          },
          "requires": {
            "$id": "#/properties/requires",
            "type": "array",
            "title": "The Requires Schema",
            "items": {
              "$id": "#/properties/requires/items",
              "type": "string",
              "title": "Project Name Schema",
              "description": "Reference to another project by name",
              "default": "",
              "examples": [
                "Frontend",
                "Backend"
              ],
              "pattern": "^(.*)$"
            }
          }
        }
      }
    }
  }
}
```

Example:

```json
{
  "apps": [
    {
      "name": "Frontend",
      "path": "./Frontend/package.json",
      "framework": "node",
      "port": 3000,
      "commands": {
        "start": "run dev"
      }
    },
    {
      "name": "Backend",
      "path": "./Backend/Backend.csproj",
      "framework": "dotnet",
      "port": 5000,
      "options": {
        "*": [ "--foo bar" ]
      }
    }
  ],
  "tests": [
    {
      "name": "SampleSuccessfulTests",
      "path": "./SampleSuccessfulTests/SampleSuccessfulTests.csproj",
      "framework": "dotnet",
      "type": "unit"
    },
    {
      "name": "NodeApp",
      "path": "./SampleNodeApp/package.json",
      "framework": "node",
      "type": "unit"
    },
    {
      "name": "IntegrationTests",
      "path": "./IntegrationTests/package.json",
      "framework": "node",
      "type": "integration",
      "requires": [ "Frontend", "Backend" ],
      "rawOutput": true,
      "serial": true,
      "environment": {
        "HEADLESS": true
      }
    }
  ],
  "services": []
}
```

The most up-to-date examples can be found in the [Sample Projects](https://github.com/tom-mckinney/regi/tree/master/src/Regi.Test/_SampleProjects_) of the `Regi.Test` project.

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