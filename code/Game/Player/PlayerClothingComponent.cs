using Sandbox;
using Sandbox.UI;
using System.ComponentModel.Design;
using System.Linq;
using System.Collections.Generic;
using System;


public sealed class PlayerClothingComponent : BaseComponent
{
	/*	[ConVar.Menu( "avatar", Saved = true, ClientData = true )]
		public static string AvatarJson { get; set; }*/

	List<SceneModel> ClothingObjects = new List<SceneModel>();

	public override void OnStart()
	{
		var clothes = new ClothingContainer();
		/*clothes.Deserialize( AvatarJson );
		*/
		var allclothes = ResourceLibrary.GetAll<Clothing>();

		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Hair ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );
		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Tops ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );
		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Bottoms ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );
		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Footwear ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );
		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Skin ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );
		clothes.Clothing.Add( allclothes.Where( X => X.Category == Clothing.ClothingCategory.Facial ).OrderBy( item => Game.Random.Next() ).FirstOrDefault() );

		ClothingObjects = clothes.DressSceneObject( GetComponent<AnimatedModelComponent>().SceneModel );
	}

	protected override void OnPreRender()
	{
		foreach ( var item in ClothingObjects )
		{
			item.Update( Time.Delta );
		}
	}
}