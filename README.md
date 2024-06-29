# SETUP

In this project the command prefix is **fisc**.

To use this:
- Open the prompt inside the realease build of the executable project
- Edit the "PATH" enviroment variable and add a new variable relative to the previous folder.

# USAGE

For any informations about the verbs avaible by the program write `fisc --help`, this will give you all the informations you will need to use the program.\
Once decided the verb proceed with the option. For any informations about option avaible in the specified verb write `fisc [verb] --help`.
Some examples:
```bash
fisc databases --help
fisc group --help
```
Every option has a description that explains what it does. If it requires input, the description will specify what is needed.

## Some General Examples
### Database example
```bash
fisc databases -a [identifier] [provider] [connetionstring]
fisc databases list
```
By default, this creates an SQLite database to save every scan.
- So the first thing i add is the identifier, that is the name,
- Next is the provider such as SQL or PostgreSQL
- Last the connection string.

The second command displays all the databases saved.
