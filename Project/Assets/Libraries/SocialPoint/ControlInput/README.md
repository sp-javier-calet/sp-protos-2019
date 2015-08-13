Control Input
============================

A helper class to read inputs from ControlFreak and GamePads
All inputs have to be defined on unity's project settings inputs, touchZones and touchSticks from ControlFreak should have enabled the "Enable GetButton()" toggle and the name of the inputs that have been defined on unity's inputs manager.

An example of input would be:
Submit, Jump, Vertical axys

Submit input can be activated through: key enter, joystickbutton14, a touchzone with the enableGetButton asigned to "Submit", so no more `#if UNITY_EDITOR Input.GetKey(KeyCode.ENTER)`.

You can add an Action to the button's events: ButtonJustPressed, ButtonJustReleased or check it's state like default Unity's Inputs: Pressed, JustPressed, JustReleased
