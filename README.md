# LifxImageScript
Command line tool that lets you script a modest light show for your LIFX lights using only an image editor. Makes use of the LIFX LAN protocol.

![Example recording](/ReadmeAssets/recording.gif)

## Image Scripts
An image script is just a normal image file that's interpreted in a special way. Horizontal rows of pixels represent lights and vertical columns of pixels represent frames. An image is read from left to right one frame at a time. The RGB color values in a frame are converted to HSV color values and sent to the corresponding light. Frame rate is configurable up to 20 fps, the max recommended send rate of the LIFX LAN protocol.

The following image script creates a red orb that bounces back and forth between four lights. Note that the image has been enlarged for display purposes. The working image script has much smaller dimensions, with a height of only four pixels.

![Bouncing orb effect](/ReadmeAssets/bouncing-orb-effect.png)

In the next image script, the red orb bounces at an accelerating rate until exploding in a burst of yellow light that trails off with a glimmering effect.

![Exploding orb effect](/ReadmeAssets/exploding-orb-effect.png)

Below is the actual image script for the light show recording at the top of the page. The red bouncing orb and exploding orb effect are used by this script. The image is best viewed with high magnification.

![Bouncing and exploding orb script](/ReadmeAssets/script.bmp)

## Command Line Reference

### Example
The following command executes a script named `hello-world.bmp` for 3 lights at 20 fps, repeating until it's stopped by key press. Lights are specified by IP address and their order corresponds to the order of horizontal rows in the image script. Light `192.168.1.89` is mapped to the topmost horizontal row and so on. The `--smooth` flag indicates that lights should transition their color and brightness smoothly between frames.

`dotnet LifxImageScript.dll --path "hello-world.bmp" --lights 192.168.1.89 192.168.1.88 192.168.1.90 --fps 20 --smooth --repeat`

### Parameters
<dl>
  <dt>--path</dt>
  <dd>Path of image script.</dd>
  
  <dt>--lights</dt>
  <dd>IP address list of lights. Order is important. The first light maps to the topmost horizontal row of image script.</dd>
  
  <dt>--fps (optional, default=1)</dt>
  <dd>Frames per second. Limited to 20, the max recommended send rate of the LIFX LAN protocol.</dd>
  
  <dt>--repeat (optional, default=off)</dt>
  <dd>Repeats image script until it's stopped by key press.</dd>
  
  <dt>--smooth (optional, default=off)</dt>
  <dd>Smoothly transitions color and brightness between frames. When disabled, frame changes may create an undesirable strobe effect.</dd>
  
  <dt>--brightness-factor (optional, default=1)</dt>
  <dd>Useful for scaling down brightness when testing a script. Accepts decimal values between 0 and 1.</dd>
</dl>
