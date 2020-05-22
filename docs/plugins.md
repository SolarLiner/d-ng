# Plugins

xây doesn't come with any language or framework-related features. However, a number of plugins are officially
supported and come bundled with the project:

- [C projects](/plugins/c)
- [JavaScript/TypeScript projects with Rome](/plugins/rome) (unimplemented)

## Plugin paths

xây looks for plugin assemblies in any of these folders:

- `$DUNG_CLI_PLUGIN_PATH` if it exists
- `$XDG_DATA_HOME/xay/plugins` or, if `XDG_DATA_HOME` isn't found, `$HOME/.xay/plugins`
- xây's executable folder

The search is recursive through subfolders.

## Using plugins

Each plugin is queried for whether it can detect a project. The first plugin to do so gets used.

In the future, the user can be asked which plugin it wants used in the case several plugins detect a project.

## Types of plugins

Plugins in xây come in different types; build plugins detect whether a language or framework is present in the
project, and extension plugins can run arbitrary code on hooks (i.e. to add additional dependencies to the project).