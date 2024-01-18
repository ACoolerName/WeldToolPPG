using UnityEngine;
using UnityEngine.Animations;

namespace WeldToolMod
{
	public class WeldTool : PinTool
	{	
		public override void CreatePin(
			Vector2 anchor,
			PhysicalBehaviour a,
			PhysicalBehaviour b = null,
			float breakThreshold = float.PositiveInfinity)
		{
			WeldBehaviour weld = a.gameObject.AddComponent<WeldBehaviour>();
			UndoControllerBehaviour.RegisterAction((IUndoableAction) new ObjectCreationAction((Object) weld, "weld"));
			weld.LocalAnchor = anchor;
			weld.Other = b;
			weld.BreakingThreshold = breakThreshold;
			weld.UsedToHaveConnectedBody = (bool) (Object) b;
			this.OnPinCreation(weld);
		}
	}

	public class WeldBehaviour : PinBehaviour
	{
		[SkipSerialisation]
		public FixedJoint2D Joint;
		[SkipSerialisation]
		private ParentConstraint parentConstraint;
		private float initialBreakForce = float.PositiveInfinity;

		private Collider2D collider;

		private void Awake() => this.PhysicalBehaviour = this.GetComponent<PhysicalBehaviour>();

		private void Start() => this.CreateJoint();

		protected void CreateJoint()
		{
			this.Joint = this.gameObject.AddComponent<FixedJoint2D>();
			this.Joint.breakForce = Utils.CalculateBreakForceForCable((AnchoredJoint2D) this.Joint, this.BreakingThreshold);
			this.initialBreakForce = this.Joint.breakForce;
			this.Joint.anchor = this.LocalAnchor;
			if (this.AttachedToWall)
				this.Joint.connectedBody = this.Other.rigidbody;
			GameObject gameObject = new GameObject("Pin");
			this.SpriteChild = gameObject.transform;
			this.SpriteChild.position = this.transform.TransformPoint((Vector3) this.LocalAnchor);
			this.SpriteChild.localScale = new Vector3(1f, 1f, 1f);
			this.parentConstraint = gameObject.AddComponent<ParentConstraint>();
			this.parentConstraint.AddSource(new ConstraintSource()
			{
				sourceTransform = this.transform,
				weight = 1f
			});
			this.SyncChildPos();
			this.parentConstraint.constraintActive = true;
			int layerId;
			int sortingOrder;
			this.GetTopMostLayer((bool) (Object) this.Other ? this.Other.GetComponent<SpriteRenderer>() : (SpriteRenderer) null, this.GetComponent<SpriteRenderer>(), out layerId, out sortingOrder);
			this.SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			this.SpriteRenderer.sortingLayerID = layerId;
			this.SpriteRenderer.sortingOrder = sortingOrder + 1;
			this.SpriteRenderer.sprite = ModAPI.LoadSprite("weldTool.png");
			CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
			circleCollider2D.isTrigger = true;
			circleCollider2D.radius = 0.07142857f;
			this.collider = (Collider2D) circleCollider2D;
			gameObject.AddComponent<Optout>();
		}

		private void GetTopMostLayer(
			SpriteRenderer a,
			SpriteRenderer b,
			out int layerId,
			out int sortingOrder)
		{
			if (!(bool) (Object) b && !(bool) (Object) a)
			{
				layerId = SortingLayer.NameToID("Top");
				sortingOrder = 100;
			}
			else
			{
				int num1 = (bool) (Object) a ? SortingLayer.GetLayerValueFromID(a.sortingLayerID) : int.MinValue;
				int num2 = (bool) (Object) a ? a.sortingOrder : int.MinValue;
				int num3 = (bool) (Object) b ? SortingLayer.GetLayerValueFromID(b.sortingLayerID) : int.MinValue;
				int num4 = (bool) (Object) b ? b.sortingOrder : int.MinValue;
				int num5 = num3;
				if (num1 > num5)
				{
					layerId = a.sortingLayerID;
					sortingOrder = num2;
				}
				else
				{
					layerId = b.sortingLayerID;
					sortingOrder = num4;
				}
			}
		}

		private void SyncChildPos()
		{
    		this.parentConstraint.SetTranslationOffset(0, (Vector3)(this.LocalAnchor * (Vector2)this.transform.localScale));
    		this.collider.offset = this.LocalAnchor; //I think this fixes the joint for being invisible idk tho im kinda dumb sometimes
		}

		private void Update()
		{
			if (Global.main.GetPausedMenu())
				return;
			bool flag = (bool) (Object) this.Other && this.Other.isDisintegrated;
			if (((!((Object) this.Joint.connectedBody == (Object) null) ? 0 : (this.UsedToHaveConnectedBody ? 1 : 0)) | (flag ? 1 : 0)) != 0)
			{
				Object.Destroy((Object) this);
			}
			else
			{
				this.SyncChildPos();
				if (!(bool) (Object) this.SpriteChild)
					return;
				this.CheckMouseInput();
			}
		}

		private void FixedUpdate()
		{
			if ((double) this.initialBreakForce == double.PositiveInfinity)
				return;
			float a = 0.0f;
			if ((bool) (Object) this.PhysicalBehaviour)
				a = this.PhysicalBehaviour.BurnProgress;
			if ((bool) (Object) this.Other)
				a = Mathf.Max(a, this.Other.BurnProgress);
			this.Joint.breakForce = this.initialBreakForce * (1f - a);
		}

		private void OnJointBreak2D(Joint2D broken)
		{
			if (!((Object) broken == (Object) this.Joint))
				return;
			Object.Destroy((Object) this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if ((bool) (Object) this.SpriteChild)
				Object.Destroy((Object) this.SpriteChild.gameObject);
			Object.Destroy((Object) this.Joint);
			Object.Destroy((Object) this);
		}

		private void OnDisable()
		{
			if (!(bool) (Object) this.SpriteChild)
				return;
			this.SpriteChild.gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			if (!(bool) (Object) this.SpriteChild)
				return;
			this.SpriteChild.gameObject.SetActive(true);
		}
	}
}
