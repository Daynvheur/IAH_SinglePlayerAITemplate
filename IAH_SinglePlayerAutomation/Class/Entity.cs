using System.Numerics;

namespace IAH_SinglePlayerAutomation.Class
{
	public class Entity
	{
		public List<string> equips = new();
		public List<string> skills = new();
		public List<string> tags = new();

		public float attackDelay;
		public float attackRange;
		public Vector3 forward;

		public string? targetUniqueID;

		public string? team;
		public string? teamCustom;
		public string? type;
		public required string uniqueID;
		public string? ip;

		public int ammo;
		public int maxAmmo;

		public int maxHp;
		public int maxSp;
		public int sp;
		public int hp;
		public int xp;
		public int xpNeeded;

		public Vector3 position;
		public bool reloading;
		public Vector3 right;

		public async Task RunAi(List<Entity> entities, Func<string, string, object, Task<APIAnswer>> BotAction)
		{
			// order by closest entity.
			entities = entities.OrderBy(entity => Vector3.Distance(position, entity.position)).ToList();

			if (entities.Count > 0) // battle mode.
			{
				float distance = Vector3.Distance(position, entities[0].position);

				var blocked = await Requests.RayCast(uniqueID, entities[0].uniqueID);

				if (distance < attackRange && blocked == false)
				{
					await BotAction(uniqueID, "rotate", entities[0].position);
					await BotAction(uniqueID, "stop", "");
				}
				else
				{
					await BotAction(uniqueID, "move", entities[0].position);
					await BotAction(uniqueID, "rotate", entities[0].position);
				}

				await BotAction(uniqueID, "attack", entities[0].uniqueID);
			}
			else
			{
				// no enemy bots. reload weapon and spin 360.
				if (ammo != maxAmmo && reloading == false)
				{
					await BotAction(uniqueID, "reload", "");
					await BotAction(uniqueID, "chat", "Reloading!");
				}

				await BotAction(uniqueID, "rotate", position + right);
			}

			/*
			 * other actions:  cancelAttack, stop, these don't have actionValue.
			 * skill: value int from 0 to 3. bot will use skill if has any.
			 */
		}
	}
}