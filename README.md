# LifxImageScript
Command line tool that lets you script a modest light show for your LIFX lights using only an image editor. Makes use of the LIFX LAN protocol.

![Example Recording](/example-recording.gif)

## Image scripts
An image script is just a normal image file that's interpreted in a special way. Horizontal rows of pixels represent lights and vertical columns of pixels represent frames. An image is read from left to right one frame at a time. The RGB color values in a frame are converted to HSV color values and sent to the corresponding light. Frame rate is configurable up to a max of 20 fps, the max recommended send rate of the LIFX LAN protocol.

The following image script creates a red orb that bounces back and forth between four lights. Note that the image has been enlarged for display purposes. The working image script has much smaller dimensions, with a height of only four pixels.

![Red Bouncing Orb Example](/example-script-enlarged1.png)


