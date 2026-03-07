using System;
using System.Numerics;

public enum ButtonPressType {
	None      = 0,
	OnClick   = 1,
	OnRelease = 2,
}

public class Button : UIElement {
	public event Action    Clicked  = delegate {};
	public event Action    Released = delegate {};
	public event Action    Pressed  = delegate {};
	public Vector2 		   Size;
	public ButtonPressType PressType = ButtonPressType.OnClick;

	private bool clicked = false;

	public bool IsClicked => clicked;

	public void Click() {
		clicked = true;
		
		OnClick();
		Clicked();

		if (PressType == ButtonPressType.OnClick) {
			Pressed();
		}
	}

	public void Release() {
		if (!clicked) return;

		clicked = false;

		OnRelease();
		Released();

		if (PressType == ButtonPressType.OnRelease) {
			Pressed();
		}
	}

	protected virtual void OnClick()   { }
	protected virtual void OnRelease() { }
}