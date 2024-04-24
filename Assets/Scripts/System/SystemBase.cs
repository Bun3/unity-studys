using System.Collections.Generic;

#if UNITY_EDITOR
using System;
using System.IO;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
#endif

namespace zc.system
{
	public abstract class SystemBase<T> where T : SystemBase<T>, new()
	{
		public static T instance { get; protected set; } = new();
		
		protected abstract void Initialize_Internal();
		protected abstract void Release_Internal();
		
	}
	
#if UNITY_EDITOR
    
	public class SystemEditor : OdinMenuEditorWindow
	{
		[MenuItem("System/System Editor")]
		public static void ShowWindow()
		{
			GetWindow<SystemEditor>();
		}

		private void OnInspectorUpdate()
		{
			Repaint();
		}

		protected override IEnumerable<object> GetTargets()
		{
			return base.GetTargets();
		}

		protected override void OnEndDrawEditors()
		{
			base.OnEndDrawEditors();
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			var tree_ = new OdinMenuTree();

			var scriptFiles_ = Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts/System/Systems"), "*.cs", SearchOption.AllDirectories);

			foreach (var filePath_ in scriptFiles_)
			{
				// 파일 이름에서 클래스 이름 추출
				var className_ = Path.GetFileNameWithoutExtension(filePath_);

				if (Type.GetType($"zc.system.{className_}") is { BaseType: { } baseType_ })
				{
					// 클래스 이름으로 트리에 추가
					tree_.AddObjectAtPath(className_, baseType_.GetProperty("instance")!.GetValue(null, null), true);	
				}
			}
			
			return tree_;
		}
	}

#endif
	
}

