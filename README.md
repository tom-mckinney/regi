# Regiment

## Installation

### NuGet

```powershell
dotnet tool install -g regi
```

### Building from source code

Windows:

```powershell
.\rebuild.cmd
```

Mac/Linux:

```powershell
./rebuild.sh
```

## Configuration

Create a file named `regi.json` in a top-level directory.

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