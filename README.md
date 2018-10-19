# LIFX Animator
Command line tool that animates your LIFX lights using sequences you create with an image editor. Makes use of the LIFX LAN protocol.

![Example recording](/Readme/recording.gif)

## Sequences
A sequence is described by an RGB image file (transparency byte is ignored). Pixel rows correspond to lights and pixel columns correspond to frames. A frame describes a color for each light. The frame corresponding to the leftmost pixel column is rendered first, then remaining frames are rendered left to right, in sequence. Frame rate is configurable up to a maximum of 20 fps, a limitation of the LIFX LAN protocol.

### Example 1
This sequence creates a red orb that bounces between four lights. Note that the image has been enlarged for display. The original pixel dimensions were 88 x 4.

![Bouncing orb effect](/Readme/bouncing-orb-effect.png)

### Example 2
This sequence creates a red orb that explodes with a shimmer effect. Note that this image has also been enlarged for display.

![Exploding orb effect](/Readme/exploding-orb-effect.png)

### Example 3
This is the actual sequence used for the GIF above.

![Bouncing and exploding orb script](/Readme/script.bmp)

## Command Line Reference

### Example
The following command executes a sequence named `sequence1.bmp` for 2 lights at 20 fps, repeating until stopped by key press. The top pixel row of the sequence image maps to light `192.168.1.89` because it is ordered first.

`dotnet LifxAnimator.dll --path "sequence1.bmp" --lights 192.168.1.89 192.168.1.88 --fps 20 --repeat-count -1`

### Parameters
<dl>
  <dt>--path</dt>
  <dd>Path of sequence image. Pixel rows correspond to lights and pixel columns correspond to frames.</dd>
  
  <dt>--lights</dt>
  <dd>Space-separated list of IP addresses. Order is important. The first light maps to the topmost pixel row of the sequence image.</dd>
  
  <dt>--fps (optional, default=1)</dt>
  <dd>Frames per second. Limited to 20, the max recommended send rate of the LIFX LAN protocol.</dd>
  
  <dt>--repeat-count (optional, default=0)</dt>
  <dd>A negative number repeats until stopped.</dd>
  
  <dt>--smooth-transitions (optional, default=off)</dt>
  <dd>Smoothly adjust color and brightness when transitioning frames.</dd>
  
  <dt>--brightness-factor (optional, default=1)</dt>
  <dd>Scales brightness so you don't need sunglasses while testing. Accepts decimal values between 0 and 1.</dd>
</dl>

## Download
LIFX Animator requires the .NET Core Runtime, which is available for Windows, Linux, and macOS.
1. [Download .NET Core Runtime](https://www.microsoft.com/net/download)
2. [Download LIFX Animator](https://github.com/galehouse5/LifxAnimator/releases/latest)
