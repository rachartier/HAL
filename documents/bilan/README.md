# Bilan sur HAL

Contenue
--------

- [Partage des tâches](#partage-des-tâches)
- [Retour sur experience](#retour-sur-experience)


Partage des tâches
-------------------

Dans cette partie nous détaillerons le travail de chacun des participants aux projets ainsi que leur investissement au sein de HAL.

Deux participants actifs au projet:
- Clément ALLAVENA
- Raphaël CHARTIER

Raphaël s'est occupé de la création du client et du serveur, leur implémentation et le bon fonctionnement de ces derniers. Il a également fournis la solution qui permet aux clients de lire et d'interpréter différents types de plugins automatiquement.

Clément s'est occupé de l'intégration du serveur et du client pour qu'ils soient conforment à l'infrastructure du parc. Il s'est aussi occupé de fournir et de maintenir les conteneurisations liées aux projets. Il a également tenté de fournir une solution du serveur qui s'est avéré inutile par la suite dû à une meilleure solution trouvée par Raphaël.

Architecture globale
--------------------

## Bases de données
Pour les bases de données, nous avons eu un grand choix car c'était à nous de l'implementer dans HAL.
On a tout d'abord choisi de faire une base de données orientée document. Nous avons choisi cette technologie car nous n'avons pas vraiment besoin de faire des recherches dans la base, et une base avec des documents et plus optimisés pour stocker une grande quantitée d'informations dans une seule base. On est donc parti sur MongoDB, pour l'instant nous en sommes très content.

Nous avons aussi utilisé une autre base, InfluxDB, pour avoir une base de données avec des "timestamps", qui permet d'avoir des ajouts temporalisés, et grace à ça, il est alors facile de construire des graphs avec les données des plugins.

Nous voulons dans le futur avoir une solution qui réunira ces 2 bases de données pour n'en former plus qu'une, pour éviter la duplication d'informations, qui reste inutile.

## API REST

Notre API est très sommaire, car elle ne possède que 2 entrypoints:
+ Récupérer les résultats des plugins
+ Récupérer les résultats des plugins par nom

Notre API n'est pas vraiment utilise à notre projet dû aux nombres enormes d'informations envoyées aux bases de données.
Grafana permet déjà de visualiser les données et les informations voulus en se connectant directement à notre base de données InfluxDB.

Elle a été faite dans un but éducatif, et elle nous a permit d'apprendre de nouvelles technologies sur la création d'API et le lien que peuvent avoir les différentes applications avec elle. 

## Vues

Nous utilisons Grafana, qui est un outil permettant de visualiser ses données dans un navigateur internet. L'avantage est que la liason entre les bases de données est déjà implementées dans leur outil, ce qui permet d'avoir très facilement des retours, de filtrer les résultats des plugins et d'avoir vraiment un visuel (comme des courbes, nuages de points ...). De plus, le client pourra, si il le souhaite, créer ses propres graphiques, ce qui permet d'avoir une grande modularitée.

Retour sur experience
----------------------

Dans cette partie nous redigerons un retour personnel sur notre experience au sein du projet HAL.

HAL a démarré avant les vacances de l'été 2019, soit fin juin, début juillet 2019.

HAL nous a permis d'en apprendre d'avantages sur la tenue de route d'un projet dans son intégralité, nous avons vu la naissance de HAL jusqu'à sa première introduction en production le 22 janvier 2020 avec un peu de retard par rapport au planning prévue, ou le premier déploiement était prévu pour fin décembre 2019.
Le fait d'avoir obtenue des résultats satisfaisant ainsi que d'avoir eu le privilège d'avoir pu présenter notre projet au MUG de Clermont-Ferrand, a été pour nous une grande fierté.

Nous allons continuer ce retour sur expérience avec une partie personnelle du retour.

__Clément__:
HAL m'aura appris beaucoup que ce soit en C# ou en intégration, avec mes ratés en C# et avec le serveur j'ai appris sur les méthodes de connexions, notamment avec les Sockets natifs de C# et leur méthode de gestion asynchrone. Mais là ou j'aurais le plus appris c'est dans l'intégration, j'ai pu apprendre à utiliser l'outil Docker et ses conteneurs et ainsi pouvoir fournir des solutions viables pour l'infrastructure du parc.

__Raphaël__:
HAL m'a permit de travailler la rigueur dans le code, dans ce qu'il y à faire et comment implémenter les différentes fonctionnalitées. J'ai aussi apprit à rédiger un programme qui doit être robuste, avec le moins de failles possibles, ce qui n'est pas toujours simple.

Le fait d'avoir comme contraintes une liste de plugins de différents types, comme scripts, dll, so... Avec tous des langages différents (Ruby, Python, C, C#, C++, Go...) et des langages gérées par l'utilisateur lui même a été un vrai casse-tête. En effet, trouver une solution propre, portable et qui marche était compliqué. Avoir réussi à en trouver une et à l'implementer a permis d'énormément apprendre sur le Marshal en C#, comment fonctionnait les DLL (classique et dotnet), comment lier le tout et surtout, comment faire une API simple pour l'utilisateur si jamais il veut rajouter différents types de plugins (par exemple PHP, Lua, Lisp...).

De plus, notre client et serveurs ont été pensés pour être le plus asynchrone et multi-threadés que possible, ce qui a rendu la tâche plus complexe mais qui a permit d'atteindre de grosses performances à moindre coût. L'utilisation et l'utilisation des threads, threadpools et de l'asynchrone en C\# m'a énormément appris sur comment marche ces derniers, dans quel contexte les utiliser, pourquoi les utiliser, comment géré les sections critiques et comment optimiser le code pour le bon fonctionnement de ces derniers.

L'autre gros casse-tête était la création du serveur. Dès le début on nous a dit qu'il devait au moins tenir 300 clients, mais très vite c'est monté à plus de 3000. Il a donc fallut trouver une solution pour avoir un serveur performant, robuste, qui ne crash pas et qui permet de une gestion très basique des plugins.

Tout ça m'a aussi permit alors d'apprendre à mieux réfléchir pour résoudre des problèmes et à prendre des initiatives quant à quoi utiliser et à quoi faire pour les résoudres. 


Globalement, nous sommes très satisfais du projet, et nous souhaitons bien entendu le faire completement marcher avant la fin de l'année scolaire.
Le seul point posifit serait que nous n'avons pas pu complétement suivre le cahier des charges, car notre client rajoutait tous le temps des demandes, ce qui nous a donné beaucoup de travail de remodelage de l'architecture, de gros changements dans le programme et donc par conséquent du retard. Nous aurions aimés soit avoir un cahier des charges complet, soit pouvoir refuser les demandes, mais nous n'avons pas osé.
