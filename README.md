AudioBridge plug-in for Unity
=============================

**AudioBridge** is an external audio processing plug-in for Unity.
It analyzes the external audio input and provides audio spectrum data for
Unity apps. It is highly optimized and uses low-latency audio APIs to process
input. And therefore you can use this plug-in to make well-synchronized
audio-visual apps.

At the moment AudioBridge only supports Mac OS X.

Demo
----

There is a demo project in [the test branch]
(https://github.com/keijiro/unity-audiobridge/tree/test) of this project.

![screenshot](http://keijiro.github.io/unity-audiobridge/screenshot.png)

For the detailed usage, see this demo project.

Usage
-----

1. Before launching Unity, select an audio interface for capturing signal
   in the system sound preference.
2. Import **UnityAudioBridgePlugin.bundle** into '**Plugins**' folder in
   your project.
3. Import **AudioBridge.cs**.
4. Add **AudioBridge** script to a game object.

You have to restart Unity to switch to another audio interface.

There are three parameters in the AudioBridge script.

- Band Type - specifies the number of octave bands.
- Sensibility - specifies how fast the analyzer reacts to the input.
- Internal Mode - analyzes the audio output from Unity instead of the
  external audio input.

The analyzer puts the result into two arrays.

- float [] levels - The levels of each octave band.
- float [] meanLevels - The mean level for a short period of time.
  Use the *Sensibility* parameter to change the duration.

Source code
-----------

The source code for the native module is stored in another repository.

[UnityAudioBridgePlugin for OS X]
(https://github.com/keijiro/UnityAudioBridgePlugin)

License
-------

Copyright (C) 2013 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
