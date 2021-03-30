using System;
using UnityEngine;
using UnityEngine.UI;

namespace VeinPlanter
{
    class TestCode : ManualBehaviour
	{
		// Token: 0x02000205 RID: 517
		// Token: 0x06000FC4 RID: 4036 RVA: 0x000B8B6E File Offset: 0x000B6F6E
		public override void _OnCreate()
		{
		}

        // Token: 0x06000FC5 RID: 4037 RVA: 0x000B8B70 File Offset: 0x000B6F70
        public override void _OnDestroy()
		{
		}

        // Token: 0x06000FC6 RID: 4038 RVA: 0x000B8B72 File Offset: 0x000B6F72
        public override bool _OnInit()
		{
			return true;
		}

        // Token: 0x06000FC7 RID: 4039 RVA: 0x000B8B75 File Offset: 0x000B6F75
        public override void _OnFree()
		{
			this.inspectPlanet = null;
			this.veinGroupIndex = 0;
			this.showingAmount = 0L;
		}

        // Token: 0x06000FC8 RID: 4040 RVA: 0x000B8B8D File Offset: 0x000B6F8D
        public override void _OnOpen()
		{
			this.Refresh();
		}

        // Token: 0x06000FC9 RID: 4041 RVA: 0x000B8B95 File Offset: 0x000B6F95
        public override void _OnClose()
		{
			this.counter = 0;
		}

        // Token: 0x06000FCA RID: 4042 RVA: 0x000B8BA0 File Offset: 0x000B6FA0
        public override void _OnUpdate()
		{
			if (this.inspectPlanet == null)
			{
				base._Close();
				return;
			}
			PlanetData.VeinGroup veinGroup = this.inspectPlanet.veinGroups[this.veinGroupIndex];
			if (veinGroup.count == 0)
			{
				base._Close();
				return;
			}
			if (this.counter % 3 == 0 && this.showingAmount != veinGroup.amount)
			{
				this.showingAmount = veinGroup.amount;
				if (veinGroup.type != EVeinType.Oil)
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					this.veinProto.name,
					"储量".Translate(),
					veinGroup.amount.ToString("#,##0")
					});
				}
				else
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					this.veinProto.name,
					"产量".Translate(),
					((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"),
					"/s"
					});
				}
			}
			this.counter++;
		}

        // Token: 0x06000FCB RID: 4043 RVA: 0x000B8D1C File Offset: 0x000B711C
        public void Refresh()
		{
			if (this.inspectPlanet == null)
			{
				return;
			}
			PlanetData.VeinGroup veinGroup = this.inspectPlanet.veinGroups[this.veinGroupIndex];
			if (veinGroup.count == 0)
			{
				base._Close();
				return;
			}
			this.veinProto = LDB.veins.Select((int)veinGroup.type);
			if (this.veinProto != null)
			{
				this.veinIcon.sprite = this.veinProto.iconSprite;
				this.showingAmount = veinGroup.amount;
				if (veinGroup.type != EVeinType.Oil)
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					this.veinProto.name,
					"储量".Translate(),
					veinGroup.amount.ToString("#,##0")
					});
				}
				else
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					this.veinProto.name,
					"产量".Translate(),
					((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"),
					"/s"
					});
				}
			}
			else
			{
				this.veinIcon.sprite = null;
				this.showingAmount = veinGroup.amount;
				if (veinGroup.type != EVeinType.Oil)
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					" ? ",
					"储量".Translate(),
					veinGroup.amount.ToString("#,##0")
					});
				}
				else
				{
					this.infoText.text = string.Concat(new string[]
					{
					veinGroup.count.ToString(),
					"空格个".Translate(),
					" ? ",
					"产量".Translate(),
					((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"),
					"/s"
					});
				}
			}
		}

		// Token: 0x04001365 RID: 4965
		[SerializeField]
		private UIVeinDetail parent;

		// Token: 0x04001366 RID: 4966
		public RectTransform rectTrans;

		// Token: 0x04001367 RID: 4967
		[SerializeField]
		private Image veinIcon;

		// Token: 0x04001368 RID: 4968
		[SerializeField]
		private Text infoText;

		// Token: 0x04001369 RID: 4969
		public PlanetData inspectPlanet;

		// Token: 0x0400136A RID: 4970
		[NonSerialized]
		public int veinGroupIndex;

		// Token: 0x0400136B RID: 4971
		private long showingAmount;

		// Token: 0x0400136C RID: 4972
		private VeinProto veinProto;

		// Token: 0x0400136D RID: 4973
		private int counter;
	}
}
