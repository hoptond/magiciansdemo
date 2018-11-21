# ONE MAN ARMY: LESSONS LEARNED FROM SOLOGAMEDEV

By Daniel Hopton

### INTRO

![The game's main menu](https://i.postimg.cc/qJB1N7Mf/main-menu-shot.png)

Around four and a bit years ago, I found myself in a position where I had quite a lot of free time. As I figured that this was as good an opportunity as any, I decided to do something I had always wanted to do, but lacked the determination: make video games.

I had spent time making games before this project, but previously I had only ever touched engines where there was no real coding required, such as RPGMaker or GameMaker. My younger self also had difficulty implementing the things I wanted to due to the limitation of those engines and lacked the patience to see the projects through to the end.

This time, I decided to write a game in a fully fledged programming language. I chose C#, as it was a good language for beginners but flexible enough to allow me to make whatever I wanted without running into the limitations or annoyances I’d found in something like GameMaker. I also chose the MonoGame framework (an open source implementation of the XNA framework), as it was supported by Visual Studio and had around half a decade’s worth of documentation and use.

I spent most of my time on this project, from early 2014 until late 2018, where I joined [Mayden Academy](https://mayden.academy) in Bath, UK, to learn how to program professionally.

At present, the game isn’t finished. Despite having written, scripted and tested the 20 hour main quest and some additional quests, I do not consider it close to finished. Sadly I no longer have the time to continue working on the game but I didn’t want to just leave it there, either. I wanted to show what I’d made and the things I’d learned. Thus, I decided to release part of the source code accompanied by a video demonstrating my game’s systems and this essay, which will focus on how the project went as a whole. I intend to release cleaned up build, including fixes and vital changes, at some point in the near future.


### THE CONCEPT

![The player character standing outside the Academy's main doors.](https://i.postimg.cc/3x6WJyRC/outside-acad-shot.png)

The game, tentatively named “Magicians”, is a Role Playing/Adventure Game set at the Torras Academy of Magic. In the game, the player takes the role of a new student as they navigate through the Academy, make friends and foes, learn a wide variety spells, and reunite a water spirit with his missing hand to prevent world ending disaster.

The game at the beginning was heavily inspired by the Persona series. For those unfamiliar with the series, it revolves around teenagers fighting monsters called Shadows using their superpowers, called Personas. The gameplay consists of two phases: during the day, players attend classes at their character’s high school, make friends and contacts called “Social Links”, buy stronger equipment, and improve their other abilities to form more powerful personas. During the night, players use their personas to clear out dungeons and collect money and experience, as you would in any other RPG. Translating that to a Magic School, where superpowers were part of the curriculum, was an obvious choice. 

At the time I started working on this, the only game that was similar to what I was going to make was [Academagia](http://academagia.com/). However, Academagia is entirely text based and is very obtuse for new players. I decided to make something that played and controlled more like a JRPG, such as Earthbound or Pokémon. 

I decided to cut out the life simulation elements before I started work on the main chunk of the game. I realised that I simply didn’t enjoy developing or playing them, and having to try and balance two games together and have them overlap meaningfully was beyond the scope of my abilities at the time. As a result, the final product is closer to a standard RPG whose characters are wizards of a chosen element. This was for the best: more focus on fewer features would produce a better end result, especially for a one person project.


### WHAT WENT RIGHT

![The first dungeon in the game.](https://i.postimg.cc/G3JBJh7d/dungeon-shot.png)

**1. Making something that was enjoyable**

One of the issues I’d bumped into before when making games was that, once I’d made something I could actually interact and play with, I would soon lose motivation as what I’d made wasn’t yet engrossing or fun to play. This didn’t happen with Magicians. The RPG formula I had chosen was comparatively simple, but time tested.

Enjoying the game was a necessity for the project to continue. I had to do an overwhelming majority of the playtesting, and thus had to run certain scenes or functions repeatedly until they worked in the way I wanted them to. If the highs of game development had not outweighed the lows, it is possible that I would have burned out long before reaching this point.

It wasn’t just my enjoyment that I valued: seeing my tester’s reactions to the events of the story and and listening to what they thought of the game was something to look forward to. To give myself notable milestones to reach and keep up regular development, I divded my game into chapters, much like those of a book. Having a clear end goal to reach helped me work through the less interesting, tedious parts of development.


![A room in the Pipeworks, my favourite area in the game.](https://i.postimg.cc/gJbJ8f3x/pipeworks-shot.png)

**2. Identifying problems**

As I was effectively performing the work of many people, I had to keep track of around a dozen different things at once while developing. Eventually, it proved to be too hard to keep these systems together, and this slowed development down. Whenever I felt that the problems with my game’s design (be it code or gameplay) grew too large, I’d open Word and write out everything that I felt was preventing me from meaningfully advancing. After I’d done this, I went through each problem in turn and then tried to address it with a potential fix. I wrote about 10 of these documents throughout the project.

The uplifting thing was that the vast majority of the problems that I’d previously thought were unassailable could be solved. Some issues were fixed with by refactoring the code (such as adding a unified class to load game data such as items and characters), while others could be solved by rebalancing the content (such as redesigning the maps to make playing the levels more enjoyable). These solutions allowed me to get over the slumps in productivity and continue to make headway in what was a fairly hefty project.

![The game's general store.](https://i.postimg.cc/8ctz8c9v/shop-shot.png)

**3. Listening to feedback**

Throughout the project I had around four people who played my game and offered me feedback on how to improve it. Though their critique of my game was soul-destroying at times (in particular one tester who seemed to enjoy deliberately breaking the game beyond what a normal user might try and accomplish), I ended up producing a better and more enjoyable game as a result.

One of many features I did not originally include, but then included based on feedback was the Sparks spell. Initially, when a character ran out of spell points and they had no items they could use, they were dead weight. The player had to use the Wait command to skip their turn. My testers found this frustrating, from what I perceived was an inability to do anything meaningful. Sparks is a spell that is castable only when the player does not have enough spell points to cast their least expensive spell. It does a small, non-scaling amount of damage to a single target. By itself it is weak, but it gave the players a reserve action when they were low on resources and a chance to pull off clutch victories.

The other important thing was to collect as much feedback from as many different types of player as possible. Though I gathered around five playtesters in total, each gave me a different, exhaustive list of feedback to the point where I had to give them each a section in my feedback file.

The first playtester played my game the most, and from him I gained the most valuable feedback, motivating me to continue with the project. Most of his suggestions were QOL changes, such as restoring the last party composition after a cutscene had played out. Often, he simply tried things that I hadn’t, as he did not instinctively know how to progress through the game.

The second playtester did not get far into Magicians’ story, as he spent the majority of the time trying to completely break the game. Most of the issues he discovered were exploits. He found a way to force enemies to spawn more frequently and therefore collect gold and experience faster than intended. He also discovered several sequence breaks that allowed him to trigger events in the story in an incorrect order. I had to write a new system to fix the former, and patch many exploits to solve the latter.

The third playtester had an input bug I couldn’t recreate under any circumstances, forcing me to add a workaround (advancing past a certain screen if no key has been pressed in 10 seconds) until I could identify the cause. Further to the “get as many different playtesters as possible” piece of advice, I learned that you should also run your build on as many different machines as possible. On my main machine, the game would automatically suppress certain exceptions while on a different machine it would crash. So far, I have been unable to identify the cause.

The fourth playtester was someone who rarely played videogames, let alone videogames of the genre (his interest died completely after seeing the turn based combat). The fifth playtester had little time to play and ultimately ran into an unreproducable crash, but his feedback was helpful as it it raised some of the points noted by the other playtesters.

![Tiled's UI.](https://i.postimg.cc/WNZqz70D/editor-screenshot.png)

**4. Good selection of tools**

Throughout the project I mostly used five tools: Visual Studio (code), Notepad++ (simple text editor for data and strings), Audacity (audio), Tiled (map editor) and Paint.net (art). I was familiar with three out of five of these tools, and eventually learned the other two to a point where I was as proficient with them as I was with the others.

VS 2010 (later 2015) is a decent IDE for working with C based languages. I made great use of its live debugging features, foremost of which was the Edit and Continue function. It allows you to write and rewrite code while in break mode and execute it as if it were written at compile time. Being able to see what was happening line by line was very helpful in diagnosing bugs, particularly in large segments of code.

Later, when I migrated to Linux, I used [MonoDevelop.](https://www.monodevelop.com/) While lacking certain features from VS, it did have a useful Code Analysis tool that identified unused variables, redundant conditionals and other changes that could be made to my code. This was most helpful towards the end of the project, where making my codebase more readable was a priority.

[Paint.net](https://www.getpaint.net/) is great. It was simple enough to learn quickly but powerful enough to do more complex things, such as layers and opacity. It was suitable for editing both pixel art and more complex work you might typically do with photoshop. Furthermore, I was able to download several useful plugins for it that helped me speed up my work. The animation viewer plugin helped me spot sprite errors and quickly rectify them, saving me hours of effort trying to fix them by eye.

[Notepad++](https://notepad-plus-plus.org/) was a good tool for editing text heavy files (such as XML) when I didn’t need the behaviour modelling and error checking of an IDE. Running it alongside VS allowed me to keep my concerns separate (code in the IDE, content in the text editor), so I was not chopping and changing between the two in one editor and interpreting my workflow.

[Tiled](https://www.mapeditor.org/) was my map editor of choice. Using an existing editor meant that I did not have to re-invent the wheel and could start building my game faster than if I had elected to create my own map format. Tiled’s output was easily adaptable, allowing me to create a template containing all the common tile and object layers that I would use throughout every map in my game.

![I had to make this UML chart by hand. It was not very fun.](https://i.postimg.cc/nrkCH0jC/code-diagram.png)


**5. Flexible objects and classes**

Though my self taught legacy code is a little questionable in places, I did make some smart choices that made working on the project easier. One of these was the separation of various common features (such as images, collision boxes, movement) into classes called “components”, which could be reused throughout the project. Taking the [entity-component](https://gameprogrammingpatterns.com/component.html), I also extended this to UI elements. As a result, my game’s code and structure was entirely object oriented.

An instance of the Entity class is a thing that represents a single object in the world. Aside from some identifying information such as a name, position and object ID, the Entity is made of several main components: a Sprite class, a Bounds class, a Movement class, a Behaviour class, and an associative array of events. The important thing is that the Entity does not need all of these to be functional, as functionality would be determined by what components the entity had.

For example, an entity with an Events list and a Bounds component would most commonly be a hotspot (an area which triggers an action when your character walks over it) while an entity with a bounds and sprite would be a background object, such as a chair or shelf. I could quickly and easily create any kind of entity the game required, just by feeding it the correct components.

Some inheritance was used for most of the humanoid or “sentient” entities: ones that could walk around and in most cases talk to the player. The Walker class inherits from the Entity class but holds an additional array of sprite references corresponding to each cardinal direction.

The components were flexible enough that they could be reused in other areas, therefore reducing the amount of code that I had to write and maintain. The objects that represented each character during a fight also possessed Sprite and Bounds components, with no code required to identify if any component was being used in a Battle scene or a World scene.


### WHAT WENT WRONG

![Really, don't make an RPG. You'll be here for a while.](https://i.postimg.cc/Pxbq42M2/forest-shot-2.png)

**1. Poor choice of genre**

Before I started Magicians, I had made two small projects: a prototype of a game where you played a rude van driver who tailgated people on the motorway and a pseudo pacman clone I did manage to finish. For my first real project, I reasoned that building a 2D RPG with no complex physics or systems would be an achievable goal.

_It was not._

On the one hand, the game’s systems were not too difficult to implement for someone who had some programming experience. Of the few years I spent working on the project, I managed to  implement most of the core systems in around 6 months. The remainder of the project was almost entirely content.

![At least it's consistent, I guess](https://i.postimg.cc/mrHkcb1x/sprites-example.png)

Above is the basic sprite sheet for the male player character. There are 8 standing frames, 16 walking frames (one for a left step and a right step) and 5 speech frames. This was the theoretical minimum for a single, fully sprited character. Every additional action a character might take (such as a character rubbing their eyes or bending down to pick something up) would require additional art on top. My game does support making characters run and switching their sprite to the appropriate frames, but I didn’t have the time to draw them.

At around the time the main quest concludes, there were approximately 60 named characters. There would have been even more, if I had time to implement the side quests I had planned. There were also many more unnamed characters, such as enemies and generic students. This doesn’t include the art I had to draw for static objects (chairs, bookshelves and tables), as well as art for the UI and visual effects. 

Aside from art, I had to implement hundreds of different items, equipment, spells and enemies to into the game. These each required their own statistics, and these statistics had to tweaked and balanced to an ensure an enjoyable experience for players. There are around 950 rooms in the game, and over 8,000 lines of dialogue.

Suffice it to say, just by picking the genre alone I had bitten off far more than I could chew. There is a reason, beyond simple nostalgia, that you see so many 2D platformers with pixel art being made by amateur game developers.

![To make pretty effects, abuse anti-aliasing.](ghost fight)

**2. Overambition**

Initially, my game had a large world you could explore beyond the Academy, which served as the world’s central point or hub. This world included the mountains, the forest, the farms, the city, the river, the Labyrinth and an ancient tower.

This was too ambitious, so I cut the areas I was least interested in. The areas that remained were the City, the Mountains, the Forest, and the River.

This was still too ambitious, so I took the elements I really wanted to include and made them part of the Academy proper. The player could not leave the Academy and its grounds for the duration of the game.

I began cutting areas out when I had finished laying out the Academy and moved on to the city. Trying to balance features between these two areas proved to be very difficult. Initially, the city was more populous while the Academy was almost devoid of anything for the player to do. I made the decision to scale down when I realised that making areas the player would revisit frequently would be a better use of my time. This would also make finishing the project more likely.

The final scope of my game was, in all honesty, still too ambitious.

There were several intertwining plot lines. The main story follows the attempts of the player and their friends to recover the plot device. Aside from this, the story also includes:
      
 - the efforts of a second, antagonistic duo who were also seeking the Plot Device.
 - a third group of low level antagonists whose actions drove a parts of the story.
 - a Master of the Academy opposed to the player’s admittance who would seek to overthrow the equally balanced council of Masters and become Grandmaster.
 - the efforts of a final year student to create the ultimate spell and pass the graduation exam,
 - a number of the Party had their own personal struggles to attend to.
 - the five duelling club members you would have to defeat to unlock special equipment.
 - the player’s own history, which would be revealed to them in a series of flashbacks through the course of game.

Tying all of these together was very difficult. Of the threads I mentioned, only the first two items made it into the game. Others partially appeared as sidequests, a few have leftover content I started but did not finish, and the rest did not appear at all. Had I been writing a novel this would have still taken up quite a bit of time, but as I was making something playable on top of it this took even longer.

![This saved me hours and hours of effort it's not even funny](https://i.postimg.cc/fTzVHnpX/debug-shot.png)

**3. Leaving til last**

Earlier, I mentioned that I had implemented most of the game’s key systems in around a half a year. I did not write every system that I should have done by the time I started the game’s content.

The biggest of these were the pathfinding system, the algorithm in which non-player characters would find their way around the world. Initially I struggled to implement a pathfinding system that worked, so I wrote a simple workaround and moved on. In the first version, entities could navigate around singular, simple objects such as rocks or trees without too much difficulty. However, when confronted with more difficult, polygonal obstacles, entities would get stuck. This meant that I had to map out areas and script cutscenes under the assumption that entities required either explicit waypoints to follow or maps had to be devoid of any complex obstacles. 

Around 10 minutes into the game, the masters of the Academy gather in the hall to determine if the player character can perform magic. Once the ritual and subsequent discussion is concluded, the other masters would then leave the hall, leaving the two the player was most familiar with to discuss amongst themselves. To avoid entities getting stuck on each other I had move the departing masters out of the room one by one. This scene became particularly infamous among my testers and I, as we had to watch the scene play out in full countless times.

Another system I did not implement during the first sixth months, but became invaluable as the project went on, was the debug pane. It was a togglable menu that allowed me alter the game state on the fly to test any scenario I required. This saved me so much time as it allowed me to skip directly to the particular sequence I wanted to test, rather than having to play up to that sequence beforehand.

Eventually, I revisited pathfinding and implemented a version of A*. Had I stuck with it and cracked the system the first time around, I would have saved myself a great deal of technical debt in the future. You will spend most of your time developing the stuff that seems simple, but the simple stuff is often the hardest, and shouldn’t be neglected.

**4. No Best Practices**

![The horror!](https://i.postimg.cc/tgqkKkJm/singleton.png)
<figcaption>Thankfully, this piece of code is no longer in the game.</figcaption>

As almost all of my programming knowledge was self taught, I picked up a few bad habits. Most of these were typical newbie mistakes such as odd naming conventions, style violations and use of antipatterns. However, the things that hindered me the most were the habits I didn’t develop and the tools I didn’t learn. My younger self couldn’t see the value in anything that did not directly contribute to building the game, so I missed out on many useful skills and tools that would have saved me hours of effort.

Version control was the biggest tool I missed out on. Often, there would be times where I broke one system while rewriting another, and I had no functioning version to go back to as I had saved over it. To find an older version of something, I had to search through manual backups I made and see if the piece of content or code still existed in any previously saved versions.

It would have also made testing deployment easier. My release method involved building the game, zipping it up, putting it on a file sharing service, and sending it out to the testers. There were a few issues with this approach. Firstly, I had to maintain entirely separate, standalone copies of the game in order to ensure they worked fresh from a new computer. Secondly, hotfixes had to be downloaded separately and copied into the directory where the user had placed the game. Thirdly, if I wanted a build where users to only be able to play up to a certain part, I would have had to remove the content from the game's directory manually. This was very tedious and wasted time that could have spent playing and recieving feedback on the game.

Unit tests were another tool that could have saved me some time and effort. When searching for the cause of a particular bug, my most common method involved using Edit and Continue to see what was happening line by line. Often, these bugs were caused by code that didn’t do what the comments or naming said it did. An automated test might have caught the bug where you could name your character ‘ ’, which was technically valid but undesirable behaviour, among other similar issues.

The final issue was that I started game development before I truly understood all of a language's features and standards. At the start of the project, I often picked the easiest or first solution I came up with for a given problem. An example of this was the dated, unweidly XML library I used to load my game's data. I put up with this library until I found and replaced it with the alternative XElement Linq library, which was much easier to read and write code for. Were I more willing to significantly overhaul legacy code, I might have replaced XML with JSON as the file format for my game's data.

**5. I Didn’t Finish**

![This room is very important.](https://i.postimg.cc/1RHtV7Xz/windy-cavern-shot.png)

The biggest gripe I have about my project is that I wasn’t able to finish it.

The game is perfectly playable. All of the game’s systems function well and are very stable, with crashes, hangs and errors being a rarity. The main quest is complete, and players are not left hanging to find out the answers to various questions raised by the story. The current maps are mostly complete, and there is no room in the game that is completely bare. The art you see in these images is typical of the art you’d see in the latest build of the game. The game has Creative Commons licensed music and sound effects, and most things you’d expect to make noise do so.

So what isn’t finished? 

![At one point, 99% of the game looked like this](https://i.postimg.cc/X7spm16P/placeholder-art.png)

Almost all of the unfinished content is art. In more detail, almost all non-player character sprites, most item and spell icons, some object sprites and almost all visual effects still rely on placeholder graphics. I may have repeated this point, but I feel I cannot repeat it enough: If you are a solo amateur game developer and you cannot draw, pick a design that requires you to draw a minimal amount of art, or hire someone to draw things for you. Your future self will thank you. 

A number of audio pieces are missing: mostly specific, obscure noises that would be difficult to mix together from free sounds.

Anything not essential for the story’s main quest was not included. Near the start of the game, there are a few atmospheric NPCs scattered throughout the Academy grounds who engage in small talk with each other about the events of the game’s story. By the end of the game, these NPCs disappear, as I did not have the time to add them to every chapter.

There’s also the final stage of polish and bugfixing that should be done before the game can released. There are several parts of my game’s story I’d like to rewrite (and rewrite and rewrite) until I felt comfortable with it being read by others. There’s also a few avoidable bugs hanging around that I’d like to squash, but had previously put off until I’d finished the more pressing tasks.


### CONCLUSION

![I felt much like they did after this was all over.](https://i.postimg.cc/2S3VDdzS/ending-shot.png)

In total, I spent around four years on this project. I had built something I could display to showcase my programming abilities. However, I had not made something I felt comfortable releasing to a wider audience, and certainly not something I could market as a product and make money as had been original goal. In total, only five people other than myself played it, and only a wider handful saw it being played.

Do I regret spending all that time on it?

No, not really. 

I learned a lot from building Magicians. The most important thing I learned was how to code, and this knowledge helped me start a proper career as a software developer. I had always been interested in how things worked under the hood, even though understanding the exact mechanics was a little tricky at times. It was these mechanics that previously

There were many instances where, when I was writing a system for the first time, I chose the easier solution over the better solution. As the project progressed, I had to revisit old code and to give it new functionality. I found myself caring about how elegant a solution I’d written, and my knowledge grew as a result of trying to refactor.

When I started smaller projects in-between working on Magicians, I had a clean slate to work from and so could approach similar problems from a new perspective. Upon returning to Magicians, I could use what I had learned to refactor the project’s existing codebase and make it easier to work with.

I feel programming is enjoyable and my code is better written when I understand exactly why something is happening. What had initially been an obstacle to something I wanted to do became something I enjoyed in itself .

There are many things that, if I could start again, I would have done differently. The biggest change would be building a smaller scale game in the same setting: one focusing on the adventures of a young apprentice and their master, rather than an entire magical school. Had I done this, not only would I have had finished sooner but have developed an appreciation for how complex certain tasks might be. This would have allowed me to complete any similar projects in the future much faster.

In the linked repository below you can see a sample of the source code for my game. Keep in mind that it was written and rewritten as part of a four-year learning experience, so the quality and style may jump around between segments of code. At some point I will also upload a cleaned up version of the most recent build of my game that you can download and play. 

Please tell me what you think. I can be emailed [here.](mailto:hoptond848@protonmail.com)

I hope reading this was an enjoyable experience.

[Code](https://github.com/hoptond/magiciansdemo)

[Demo Video](https://www.youtube.com/watch?v=CuOT8PEbfqg)

Build coming soon(ish)
