# Contributing

## Coding Conventions
### Variable Naming
* `Fields` => If field is `public`, use `camelCase`
* * If field is `private`, depending on which purpose the field is being used
* * `Encapsulated` => `m_` prefixed `PascalCase`
* * `Just Private` => `m_` prefixed `camelCase`
* `Properties` => `PascalCase` only if possible, otherwise follow the `Field` naming conventions (sans `m_` prefix)
* `Methods` => `PascalCase` only.
* * `Methods > Parameters` => `camelCase` only.
* `Classes` => `PascalCase` only
* `Constants` => `PascalCase` only unless private, but still prefer `PascalCase`.
* **Specific files :** Some files can declare their own naming conventions, as long as the global/public api is similar to this naming it's fine.

**Note :**
There could be some violations of variable namings in certain classes, unless the violations make sense in that given script/code's context i will change it.

### Comments
* Make sure that the functions, variables, properties, classes/structs, etc. is commented inline using xmldoc. The commenting won't apply to classes that are transparent / won't be directly used by code (for example, struct PropertyDrawers).
* Explanations should only be done if needed, which marks the bad parts of the code anyways (which is a lot of the bxfw lol)

---
## Resources
* Resources (such as images, sound files, etc.) should be only added when absolutely needed. (bxfw aims to be mostly code-only)
* If the resource can be done/generated using code in a trivial way it should be done that way, as long as it's not impacting the performance terribly and is trivial.
