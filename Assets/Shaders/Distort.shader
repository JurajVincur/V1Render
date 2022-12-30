Shader "Custom/Distort" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			CGPROGRAM
			#pragma surface surf Unlit vertex:vert
			struct Input {
				float2 uv_MainTex;
			};

			half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
				half4 c;
				c.rgb = s.Albedo;
				c.a = s.Alpha;
				return c;
			}

			// Properties
			sampler2D _MainTex;
			float4x4 _matrixP;
			float4x4 _matrixIP;
			float4x4 _matrixCL2W;
			float4x4 _matrixCW2L;
			float4x4 _matrixEW2L;
			float4x4 _matrixEL2W;
			float3 _escale;
			float4x4 _matrixDW2L;
			float4x4 _matrixDL2W;

			//see https://diegoinacio.github.io/computer-vision-notebooks-page/pages/ray-intersection_sphere.html
			float3 rayToIntersection(float3 direction, float3 origin) //ellipsoid local dir and local origin
			{
				float rsq = 0.25;
				float3 sphereCenter = float3(0.0, 0.0, 0.0);
				float3 ndir = normalize(direction);
				float3 orientedSegment = sphereCenter - origin;
				float t = dot(orientedSegment, ndir);

				float3 pe = origin + ndir * t;
				float3 tmp = pe - sphereCenter;
				float dsq = dot(tmp, tmp);
				float i = sqrt(rsq - dsq);

				float3 ps = origin + ndir * (t + i);
				return ps;
			}

			float3 intersectionToNormal(float3 intersection)
			{
				float3 sphereCenter = float3(0.0f, 0.0f, 0.0f);
				float3 normal = intersection - sphereCenter;
				return normalize(normal);
			}

			float3 rayToPlaneIntersections(float3 direction, float3 origin)
			{
				float3 normal = mul((float3x3)_matrixDL2W, float3(0.0f, 0.0f, 1.0f));
				float3 position = mul(_matrixDL2W, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
				float denom = dot(normal, direction);
				if (abs(denom) > 1e-6)
				{
					float3 segment = position - origin;
					float t = dot(segment, normal) / denom;
					if (t > 0.0f)
					{
						float3 intersection = origin + direction * t;
						return intersection;
					}
				}
				return float3(0.0f, 0.0f, 0.0f);
			}

			void vert(inout appdata_full v) {
				float3 localCameraDir = mul(_matrixIP, float4(v.texcoord.xy * 2.0f - 1.0f, 0.0f, 1.0f)).xyz;
				localCameraDir *= float3(1.0f, _ProjectionParams.x, 1.0f);
				float3 worldDir = mul((float3x3)_matrixCL2W, localCameraDir);
				float3 worldOrigin = mul(_matrixCL2W, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
				float3 localDir = mul((float3x3)_matrixEW2L, worldDir);
				float3 localOrigin = mul(_matrixEW2L, float4(worldOrigin, 1.0f)).xyz;
				float3 sphereIntersection = rayToIntersection(localDir, localOrigin);

				float3 sphereNormal = intersectionToNormal(sphereIntersection);
				float3 ellipsoidNormal = sphereNormal;
				ellipsoidNormal.x /= pow(_escale.x / 2.0f, 2.0f);
				ellipsoidNormal.y /= pow(_escale.y / 2.0f, 2.0f);
				ellipsoidNormal.z /= pow(_escale.z / 2.0f, 2.0f);

				float3 worldEllipsoidNormal = normalize(mul((float3x3)_matrixEL2W, ellipsoidNormal));
				float factor = -2.0f * dot(worldEllipsoidNormal, worldDir);
				float3 worldReflected = normalize(worldEllipsoidNormal * factor + worldDir);
				float3 worldIntersection = mul(_matrixEL2W, float4(sphereIntersection, 1.0f)).xyz;

				float3 worldPlaneIntersection = rayToPlaneIntersections(worldReflected, worldIntersection);

				v.vertex = mul(_matrixDW2L, float4(worldPlaneIntersection, 1.0f));
			}

			void surf(Input IN, inout SurfaceOutput o) {
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			}

			ENDCG
	}
		Fallback "Diffuse"
}