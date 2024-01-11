using System.Numerics;
using System.Text.Json.Serialization;

namespace IAH_SinglePlayerAutomation.Class.Response;

public class EntitiesResponse
{
	[JsonInclude]
	public List<Entity> entities = new();
}

public class Entity
{
	[JsonInclude]
	public List<string> equips = new();
	[JsonInclude]
	public List<string> skills = new();
	[JsonInclude]
	public List<string> tags = new();

	[JsonInclude]
	public float attackDelay;
	[JsonInclude]
	public float attackRange;
	[JsonInclude]
	public Vector3 forward;

	[JsonInclude]
	public string? targetUniqueID;

	[JsonInclude]
	public string? team;
	[JsonInclude]
	public string? teamCustom;
	[JsonInclude]
	public string? type;
	[JsonInclude]
	public required string uniqueID;
	[JsonInclude]
	public string? ip;

	[JsonInclude]
	public int ammo;
	[JsonInclude]
	public int maxAmmo;

	[JsonInclude]
	public int maxHp;
	[JsonInclude]
	public int maxSp;
	[JsonInclude]
	public int sp;
	[JsonInclude]
	public int hp;
	[JsonInclude]
	public int xp;
	[JsonInclude]
	public int xpNeeded;

	[JsonInclude]
	public Vector3 position;
	[JsonInclude]
	public bool reloading;
	[JsonInclude]
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