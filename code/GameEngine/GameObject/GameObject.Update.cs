public partial class GameObject
{
	protected virtual void Update()
	{
		if ( !Enabled || Static )
			return;

		Transform.Update();

		ForEachComponent( "Update", true, c => c.InternalUpdate() );
		ForEachChild( "Tick", true, x =>
		{
			if ( !x.Enabled || x.Static )
				return;

			x.Update();
		} );
	}

	protected virtual void FixedUpdate()
	{
		if ( Static )
			return;

		ForEachComponent( "FixedUpdate", true, c => c.FixedUpdate() );
		ForEachChild( "FixedUpdate", true, x =>
		{
			if ( x.Static )
				return;

			x.FixedUpdate();
		} );
	}
}
