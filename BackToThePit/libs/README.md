# RiskOfOptions
![Animated icon made by UnsavedTrash#0001](https://thumbs.gfycat.com/LimitedFlashyCowbird-size_restricted.gif)

An API to provide a user interface in game to interact with BepInEx ConfigEntry's.

## Currently supported options
- CheckBoxes
- Sliders and Stepped Sliders
- KeyBinds
- String Input Fields

Feature requests are welcome at my [repository](https://github.com/Rune580/RiskOfOptions).

## Getting Started
First you need to grab the latest release from the [Thunderstore](https://thunderstore.io/package/Rune580/Risk_Of_Options/).
Extract the mod to your plugins folder, and then add a reference to the dll in your project in Visual Studio. [Project->Add Reference...->Browse]

Then add to where ever you will use this.
```C#
using RiskOfOptions;
```

Next you need to add Risk Of Options as a dependency for your mod.
```C#
[BepInDependency("com.rune580.riskofoptions")]
```

Finally make sure you know how to use [BepInEx Config](https://github.com/risk-of-thunder/R2Wiki/wiki/Configuration
)

Now you're ready to start adding options.

### Adding an option
Given a `ConfigEntry<bool>`
```C#
ConfigEntry<bool> enableThing = Config.Bind(...);

ModSettingsManager.AddOption(new CheckBoxOption(enableThing);
```

Need a volume slider?
```C#
ConfigEntry<float> volume = Config.Bind(...);

ModSettingsManager.AddOption(new SliderOption(volume));
```

Every option constructor can take a Config for the example above it would be `SliderConfig`.
Say you need a slider that only goes between 60 - 130. You would do:
```C#
ModSettingsManager.AddOption(new SliderOption(limitedRangeFloat, new SliderConfig() { min = 60, max = 130 }));
```

What about a slider that goes in increments of 0.15 and is limited between 1 - 5?
```C#
ModSettingsManager.AddOption(new StepSliderOption(incrementedFloat, new StepSliderConfig() { min = 1, max = 5, increment = 0.15f }));
```

Enough about floats, let's talk about the spaghetti and meatballs, KeyBinds.
```C#
ConfigEntry<KeyboardShortcut> keyBind = Config.Bind(...);

ModSettingsManager.AddOption(new KeyBindOption(keyBind)); // This also has a KeyBindConfig but can be omitted if defaults are desired.
```
And that's it, said KeyboardShortcut will show up on the ModOptions menu.

Checkbox and Slider configs can be set with a delegate that will be used to check if said option should be disabled in the menu.
```C#
ConfigEntry<bool> disableThing = Config.Bind(...);
ConfigEntry<bool> overridenThing = Config.Bind(...); 

ModSettingsManager.AddOption(new CheckBoxOption(disableThing));
ModSettingsManager.AddOption(new CheckBoxOption(overridenThing, new CheckBoxConfig() { checkIfDisabled = Check }));

...

private bool Check()
{
    return disabledThing.value;
}
```
When `disableThing` is enabled `overridenThing` will show up as non-interactable in the menu.

"Okay that's all fine but how do I, you know, do stuff when an value is changed?"
Well thankfully `ConfigEntry`'s have this innately:
```C#
ConfigEntry<bool> toggleThing = Config.Bind(...);

toggleThing.SettingChanged += (object, args) => { Debug.Log(toggleThing.Value) };
```
Of course when an option changes the value of a passed `ConfigEntry`, the value updates in real time,
so in some cases where you are checking the value of the entry directly you don't need to do anything.

### Setting the description of the mod
```C#
ModSettingsManager.SetModDescription("Describe your mod in incredible detail over the course of the next 2 hours");
```

### Setting the icon of the mod
```C#
Sprite icon = ...;

ModSettingsManager.SetModIcon(icon);
```

# Quick Showcase
[Showcase](https://gfycat.com/GloomyShowyArrowana)

# Changelog
    2.1.0 - Added StringInputFieldOption. Configuration option `restartRequired` has been fully implemented,
    set this to true to show a restart warning when the option is modified. Description panels now scroll.

    2.0.0 - Massive Rewrite of the entire mod. Now exclusively uses BepInEx ConfigEntry's.
    Completely new UI, with a working revert button. Added Stepped Slider, and KeyBind options.
    Mods dependent on 1.0 will not work with 2.0

    1.0.4 - Quick update for SOTV.

	1.0.3 - Added R2API as a dependency because I forgot about that. Also I'm currently rewriting ROO,
	so hopefully the next update should be a pretty big one. No ETA on that, but progress is good
	so far.
