using System;
using UnityEngine;
using NUnit.Framework;
using UnityEditor.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder.Tests
{
	class RecorderSettingsTests
	{
		[Test]
		public void ImageRecorderSettings_ShouldHaveProperPublicAPI()
		{
			var recorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();

			AssertBaseProperties(recorder);

			recorder.captureAlpha = true;
			recorder.outputFormat = ImageRecorderOutputFormat.PNG;

			Assert.IsTrue(recorder.imageInputSettings is GameViewInputSettings);
			
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new CameraInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new GameViewInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new Camera360InputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureSamplerSettings());
			
			Assert.Throws<ArgumentNullException>(() => recorder.imageInputSettings = null);
			
			UnityObject.DestroyImmediate(recorder);
		}
		
		[Test]
		public void MovieRecorderSettings_ShouldHaveProperPublicAPI()
		{
			var recorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();

			AssertBaseProperties(recorder);

			recorder.captureAlpha = true;
			recorder.outputFormat = VideoRecorderOutputFormat.MP4;
			recorder.videoBitRateMode = VideoBitrateMode.High;

			Assert.IsTrue(recorder.imageInputSettings is GameViewInputSettings);
			Assert.IsNotNull(recorder.audioInputSettings);
			
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new CameraInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new GameViewInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new Camera360InputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureSamplerSettings());
			
			Assert.Throws<ArgumentNullException>(() => recorder.imageInputSettings = null);
			
			UnityObject.DestroyImmediate(recorder);
		}
		
		[Test]
		public void AnimationRecorderSettings_ShouldHaveProperPublicAPI()
		{
			var recorder = ScriptableObject.CreateInstance<AnimationRecorderSettings>();

			AssertBaseProperties(recorder);

			Assert.IsNotNull(recorder.animationInputSettings);
			
			UnityObject.DestroyImmediate(recorder);
		}

		[Test]
		public void GIFRecorderSettings_ShouldHaveProperPublicAPI()
		{
			var recorder = ScriptableObject.CreateInstance<GIFRecorderSettings>();

			AssertBaseProperties(recorder);

			recorder.numColors = 123;
			recorder.keyframeInterval = 15;
			recorder.maxTasks = 10;

			Assert.IsTrue(recorder.imageInputSettings is CameraInputSettings);
			
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new CameraInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureInputSettings());
			Assert.DoesNotThrow(() => recorder.imageInputSettings = new RenderTextureSamplerSettings());
			
			Assert.Throws<ArgumentException>(() => recorder.imageInputSettings = new GameViewInputSettings());
			Assert.Throws<ArgumentException>(() => recorder.imageInputSettings = new Camera360InputSettings());
			
			Assert.Throws<ArgumentNullException>(() => recorder.imageInputSettings = null);
			
			UnityObject.DestroyImmediate(recorder);
		}
		
		[TestCase("C:/AAA/BBB.MP4", "C:/AAA/BBB.MP4")]
		[TestCase("C:\\\\AAA///\\BBB.MP4", "C:/AAA/BBB.MP4")]
		[TestCase("AAA.MP4", "AAA.MP4")]
		[TestCase("AAA", "AAA")]
		[TestCase("Assets/AAA/BBB.MP4", "Assets/AAA/BBB.MP4")]
		[TestCase("C:/Assets/AAA/BBB.MP4", "C:/Assets/AAA/BBB.MP4")]
		[TestCase("../AAA/BBB.MP4", "../AAA/BBB.MP4")]
		[TestCase("/AAA", "/AAA")]
		[TestCase("/AAA.MP4", "/AAA.MP4")]
		[Ignore("Waiting for CI to be fixed")]
		public void OutputFile_ShouldReturnAssignedValue(string value, string expected)
		{
			var recorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();

			recorder.outputFile = value;
			Assert.AreEqual(expected, recorder.outputFile);
		}
		
		[TestCase(null)]
		[TestCase("")]
		public void OutputFile_InvalidPathShouldThrow(string value)
		{
			var recorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();

			var e = Assert.Throws<ArgumentException>(() => recorder.outputFile = value);
			Assert.IsTrue(e.Message.Contains(RecorderSettings.s_OutputFileErrorMessage));
		}

		static void AssertBaseProperties(RecorderSettings recorder)
		{
			Assert.IsTrue(recorder.enabled);
			Assert.IsNotEmpty(recorder.extension);
			Assert.IsNotNull(recorder.outputFile);
			Assert.IsTrue(recorder.take == 1);

			// Test public access
			Assert.DoesNotThrow(() =>
			{
				var b = recorder.isPlatformSupported;
			});
		}
	}
}
