- [Configuration](#configuration)
  - [Comprendre `config_global.json` et `config_local.json`](#comprendre-config_globaljson-et-config_localjson)
  - [Configurer le serveur](#configurer-le-serveur)
    - [Changement d'ip et de port](#changement-dip-et-de-port)
- [Rédaction plugins](#rédaction-plugins)
  - [Difference entre AssemblyDLL, DLL classique, shared object et script](#difference-entre-assemblydll-dll-classique-shared-object-et-script)
  - [Rédaction de plugins via langages supportés par défaut](#rédaction-de-plugins-via-langages-supportés-par-défaut)
      - [Exemple en C/C++ (DLL classique, Shared Object)](#exemple-en-cc-dll-classique-shared-object)
      - [Exemple en C# (AssemblyDLL)](#exemple-en-c-assemblydll)
  - [Exemple en GO (Script / DLL classique)](#exemple-en-go-script--dll-classique)
      - [Exemple en Python (Script)](#exemple-en-python-script)
	

	- Configurer les clients
		- Interpreteurs
			- Sous Linux 
			- Sous Windows
		- Plugins
			- Attributs 
				- activated
				- heartbeat
				- os
			- Mode "administrateur"
				- admin_rights
				- admin_username
			- Mode "differencial"
				- differencial_all
				- differencial (avec champs)
	- Configuration des clients par le serveur 
	- Configurer les sauvegardes des résultats
		- Sortie console
		- Local
		- Serveur
		- MangoDB
		- InfluxDB
		- Ajout de bases de données personnalisées


		- Via langage non supporté par défaut
			- Ajout d'un interpreteur (explications poussées)
	- Vérification des sorties des plugins 
		- Présentation du plugins_checker



Configuration
-------------
### Comprendre `config_global.json` et `config_local.json` 

2 types de fichiers de configurations sont présents:
*  config\_local.json
*  config\_global.json

`config_local.json`: sert uniquement à rajouter des configurations de plugin. Il est déposé en local dans le dossier "config" sur les *clients*, et ne sera en aucun cas supprimé ou modifié par le serveur.

`config_global.json`: sert à modifier tous ce qui est possible dans HAL. Tout est détaillés plus bas dans la documentation. Ce fichier sera distribué à tous les clients via le serveur.

Il sera impératif de rédiger son propre `config_global.json`, et de l'ajouter dans le dossier "plugins" du serveur.

### Configurer le serveur
#### Changement d'ip et de port

Pour régler la connection au serveur d'un client, il faut ajouter dans le fichier `config_local.json`, présent sur les clients:

```json
"server": {
    "ip": "<ip du serveur>",
    "port": <port du serveur>
}
```

- `ip`: l'ip d'un des serveurs HAL
- `port`: port d'un des serveurs HAL, par défaut `11000`

Rédaction plugins
-----------------

Les informations retournées par le plugin doivent être sous un format JSON.
Pour une efficacité optimal, il convient de normaliser les soties de vos plugins. C'est à dire, respecté une convention de nommage définit selon vos principes.

Chaque plugin doit être mit dans le dossier "plugins" du serveur, qui se chargera de les transmettres aux clients. De plus, une entrée doit être écrite dans le fichier config.json pour avoir les informations nécéssaire au bon déroulement du plugin. 

### Difference entre AssemblyDLL, DLL classique, shared object et script

Le manageur de plugins de HAL permet de lire différents types de DLL pour les plugins, ce qui en fait un outil extrémement modulable.

- Les AssemblyDLL sont les dll générées par la création de bibliothèques .NET, pour Windows et Linux
- Les DLL "classiques" sont celles générées par des compilateurs types C, C++..., mais uniquement pour Windows. 
- Les Shared Objects sont comme les DLL classiques, mais uniquement sous Linux.
- Les scripts peuvent être codé en n'importe quel langage de script (Lua, PHP, Bash, Python, Ruby...). Un fichier source unique doit être fournit par plugin. D'autre interpreteurs peuvent être rajoutés si ceux par défaut ne correspondent pas aux attentes. 

Ces différents format de plugins peuvent être combinés à souhait poour avoir une liste de plugins aussi personalisable que possible.

### Rédaction de plugins via langages supportés par défaut


##### Exemple en C/C++ (DLL classique, Shared Object)

Un point d'entrée est obligatoire pour l'execution du plugin en DLL classique et SO. Le retour de ce point d'entrée sera alors sauvegardé par le client. Il doit impérativement être en JSON.

``` c
char* run() {

}
```

Exemple d'un code en C (ip\_infos.c) uniquement disponible sur linux (ifaddrs.h) permettant de récupérer le nom de la carte réseau, son addresse et les données.
``` c
#include <ifaddrs.h>
#include <stdio.h>
#include <stdlib.h>

char *run(void) {
	struct ifaddrs* id;
	char* ret = malloc(1024);

	getifaddrs(&id);

	sprintf(ret, "{ \"name\": \"%s\", \"addr\": %d, \"data\": %d}", id->ifa_name, id->ifa_addr, id->ifa_data);
	
	return ret;
}
```

Il faut impérativement que la variable de retour soit allouée en mémoire (via malloc, calloc...)  
Des fonctions peuvent bien sûr être faites pour clarifier le code.

Compilation en .so sous linux:
	`gcc -Wall -Wextra -fPIC -shared ip_infos.c -o ip_infos.so`

Compilation en .dll sous windows (si plugin compatible) avec utilisation de [MinGW](http://www.mingw.org/)
	`gcc -Wall -Wextra -fPIC -shared fichier.c -o ip_infos.dll`

puis copier ip_infos.so dans le dossier plugins et rajouter une entrée dans config.json:

```json
 ...
	"plugins": {
	    ...
		"ip\_infos.dll": {
			"activated": "true",
			"heartbeat": "1",
			"os": ["linux"]
		},
	    "ip_infos.so": {
			"activated": "true",
			"heartbeat": "1",
			"os": ["windows"]
		}
		...
	}
 ...
```

##### Exemple en C# (AssemblyDLL)

Un point d'entrée est aussi obligatoire pour l'execution du plugin. Le retour du point d'entrée sera alors sauvegardé par le client. Il doit impérativement être en JSON.

```cs
namespace Plugin 
{
	public class NomPlugin 
	{
	    public string Run() 
	    {

	    }
	}
}
```

Création d'un projet dotnet:

`dotnet new classlib -n plugin`

Modifier Class1.cs pour écrire son plugin
Exemple d'un plugin pour récupérer le nom de la machine:

```cs
using System;

namespace plugin
{
    public class MachineName 
    {
	    public string Run() 
	    {
		    return $"{{\"machinename\": {Environment.MachineName}}}";
	    }
    }
}
```

Puis build la librairie:

`dotnet build`

Finalement, copier et renommer si besoin plugin.dll et le mettre ensuite dans le dossier plugins et ajouter une entrée dans config.json:

```json
 ...
	"plugins": {
	    ...
		"plugin.dll": {
			"activated": "true",
			"heartbeat": "0.5"
		}
		...
	}
 ...
```

### Exemple en GO (Script / DLL classique)

Go est intéprété en tant que langage de script dans HAL. De ce fait, il doit respecter les contraintes des autres langages de script, c'est à dire defaire sortir le JSON sur la sortie standard.

Exemple d'un plugin 

```go
package main

import (
    "fmt"
)

func main() {
    fmt.Print("{\"test\": \"reussi\"}")
}

```

Il est aussi possible de faire du Go compilé (sous forme de DLL). L'avantage étant que le client n'est pas obligé de posseder golang. 

```go
package main

import "C"

func main() {
}

//export run
func run() *C.char {
	return C.CString("{\"test\": \"reussi\"}")
}
```

Il est imépratif d'avoir le commentaire "export run" pour que cela fonctionne. De plus, il faut aussi obligatoirement mettre *C.char comme type de retour.

Compilation: `go build -o testplugin.dll -buildmode=c-shared`


##### Exemple en Python (Script)

- Créer un fichier `osinfo.py`
- Y ajouter:

    ``` python
    import platform
    import json

    osInfo = {
        "machine":platform.machine(),
        "version":platform.version(),
        "platform":platform.platform(),
        "system":platform.system(),
        "processor":platform.processor()
    }

    print(json.dumps(osInfo));
    ```

 - Déplacer le fichier dans le répertoire "plugins" du serveur