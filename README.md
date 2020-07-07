
# UnityEcsEvents.Example
An example showing the how ECS Events package can be utilized in a game.

# Note: this example has not yet been updated to use the latest version of UnityEcsEvents and Unity packages.

#### What is this?

A demonstration project for how the [ECS Events package](https://github.com/jeffvella/UnityEcsEvents) could be used in a real project. The game itself is pretty simple but the point is to illustrate methods of wiring functionality together. The project shows:

* Firing and Receiving events from a Monobehavior.
* Using events to control gameplay.
* Scene Loading/Unloading with events.
* Using prefabs from SubScenes.
* Instantiating ParticleEffects and Sounds from ECS

#### It looks like this:

![enter image description here](https://i.imgur.com/byHVBhg.gif)

#### Installation

Clone or Download the project, 'Add' the project to Unity Hub and open run in Unity 2019.3.6f1+. Once loaded, which might take a while to sort out all the required packages) open the scene 'Assets\Scenes\StartScene'.

#### Packages and Versions Used:

    "com.unity.rendering.hybrid": "0.4.0-preview.8",
    "com.unity.jobs": "0.2.7-preview.11",
    "com.unity.physics": "0.3.1-preview",
    "com.unity.burst": "1.3.0-preview.7",
    "com.unity.collections": "0.7.0-preview.2",
    "com.unity.entities": "0.8.0-preview.8"

