# LIFX Animator
Command line tool that animates your LIFX lights using sequences you create with an image editor.

![Example recording](/Readme/recording.gif)

This tool uses the LIFX LAN protocol because it's capable of updating a light's color more frequently than the HTTP API.

## Sequences
A sequence is described by an RGB image file (transparency byte is ignored). Pixel rows correspond to lights and pixel columns correspond to frames. A frame designates a color for each light. The frame corresponding to the leftmost pixel column is rendered first,  then the frame to its right is rendered, and so on. By default, each frame is displayed for 1/10th of a second before the next frame is rendered.

### Example 1
This sequence creates a red orb that bounces between four lights. (Note the image is enlarged for display.)

![Bouncing orb effect](/Readme/bouncing-orb-effect.png)

### Example 2
This sequence creates a red orb that explodes with a shimmer effect. (Note the image is enlarged for display.)

![Exploding orb effect](/Readme/exploding-orb-effect.png)

### Example 3
This is the actual sequence used for the GIF recording at the top of the page.

![Bouncing and exploding orb script](/Readme/script.bmp)

## Command Line Reference

### Example
The following command executes a sequence named `sequence1.bmp` for 2 lights at 20 fps, repeating until stopped by key press. The top pixel row of the sequence maps to light `192.168.1.89` because of its order.

`dotnet LifxAnimator.dll --path "sequence1.bmp" --lights 192.168.1.89 192.168.1.88`

### Parameters
<dl>
  <dt>--path</dt>
  <dd>Path of sequence image. Pixel rows correspond to lights and pixel columns correspond to frames.</dd>
  
  <dt>--lights</dt>
  <dd>Space-separated, ordered list of IP addresses. The first light maps to the topmost pixel row of the sequence image.</dd>
  
  <dt>--fps (optional, default=20)</dt>
  <dd>Frames per second. Limited to 20, the max recommended send rate of the LIFX LAN protocol.</dd>
  
  <dt>--repeat-count / --repeat-seconds (optional)</dt>
  <dd>If not specified then repeats until stopped by key press.</dd>
  
  <dt>--smooth-transitions (optional, default=off)</dt>
  <dd>Smoothly adjust color and brightness when transitioning frames.</dd>
  
  <dt>--brightness-factor (optional, default=1)</dt>
  <dd>Scales brightness so you don't need sunglasses while testing. Accepts decimal values between 0 and 1.</dd>
</dl>

## Download
LIFX Animator requires the .NET Core Runtime, which is available for Windows, Linux, and macOS.
1. [Download .NET Core Runtime](https://www.microsoft.com/net/download)
2. [Download LIFX Animator](https://github.com/galehouse5/LifxAnimator/releases/latest)
