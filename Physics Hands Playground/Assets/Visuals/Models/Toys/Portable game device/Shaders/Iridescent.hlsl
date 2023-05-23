void Iridescent_float(in float3 V, in float3 N, in float Noise, out float3 Iridescent)
{
	float3 k = normalize(float3(1, 1, 1));
	float t = dot(N, V) * Noise * PI * 12;
	float3 v = float3(0.5, 0.5, 1);
	Iridescent = v* cos(t) + cross(k, v) * sin(t) + k * dot(k, v) * (1 - cos(t));
}
	