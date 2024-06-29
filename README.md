#SETUP

In this project the command prefix is **fisc**.

To actually use this, you can open the prompt inside the realease build of the executable project, and write ./fisc or edit the "PATH" enviroment variable and add a new variable relative to the previous folder.

#USAGE

For any informations about the verbs avaible by the program write `fisc --help`, this will give you all the informations you will need to use the program.\
Once decided the verb proceed with the option. For any informations about option avaible in the specified verb write `fisc [verb] --help`.
Some examples:
```bash
fisc databases --help
fisc group --help
```
Every option has a description that describes, if it's an input what is needed, else what the option does.

Some General Examples
```bash
fisc databases -a [identifier] [provider] [connetionstring]
fisc databases list
```
These commands will first create a databases, by default its an Sqlite server to save every scan. So the first thing i add is the identifier, that is the name, next is the provider such as SQL or PostgreSQL at last the connection string.
The second command displays all the databases saved.
