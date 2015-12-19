// Cartoon FX
// (c) 2015, Jean Moreno

// This shader will render the particles once with a solid color
// (controlled by the 'Alpha Blended Color' property), and then
// render it with an additive blending mode.

// This allows the system to benefit from the color addition
// effect but still look good over bright background.
// Drawback is that it will cost 1 more drawcall and double
// the number of rendered particles

Shader "Cartoon FX/Alpha Blended + Additive (BaseColor)"
{
Properties
{
	_BaseColor ("Base Color", Color) = (0.5,0.5,0.5,0.5)
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category
{
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	ColorMask RGB
	Cull Off
	Lighting Off
	ZWrite Off
	Fog { Color (0,0,0,0) }
	
	SubShader
	{
		UsePass "Hidden/Cartoon FX/Particles/ALPHA_BLENDED"
		UsePass "Hidden/Cartoon FX/Particles/ADDITIVE"
	}	
}
}
