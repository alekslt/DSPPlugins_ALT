using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VeinPlanter.Model;
using VeinPlanter.Service;
using static VeinPlanter.VeinPlanter;

namespace VeinPlanter.Presenters
{

    public class VeinGardenerGroupModifyPresenter : PresenterBase
    {
        public static GameObject GUIGOVeinGroupModify = null;
        public static UIVeinGardenerGroupEdit UIVGGroupEdit = null;
        private VeinGardenerModel _veinGardenerState;
        private RectTransform ContentParent;

        float oilMax = 120f;
        float oreMultiplier = 0.000001f;

        bool IsNotValidVGState => (_veinGardenerState.localPlanet == null || _veinGardenerState.localPlanet.factory == null || _veinGardenerState.veinGroupIndex == 0);


        public VeinGardenerGroupModifyPresenter(VeinGardenerModel veinGardenerState)
        {
            _veinGardenerState = veinGardenerState;
        }

        public void Init()
        {
            var inGameWindows = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows");
            //Debug.Log(inGameWindows);
            ContentParent = inGameWindows.GetComponent<RectTransform>();
            GUIGOVeinGroupModify = VeinPlanter.Instantiate(bundleAssets.LoadAsset<GameObject>("Assets/UI/VeinGardener/UIVeinGardenerGroupEdit.prefab"));
            AttachGameObject(GUIGOVeinGroupModify, inGameWindows.transform);


            UIVGGroupEdit = GUIGOVeinGroupModify.GetComponent<UIVeinGardenerGroupEdit>();
            if (!UIVGGroupEdit.IsInit) UIVGGroupEdit.Init();

            //GUIGOVeinGroupModify.SetActive(true);

            Debug.Log("Registering callbacks for VG.GroupEdit");

            UIVGGroupEdit.UIOnVeinTypeChanged = newValue => {
                if (IsNotValidVGState)
                {
                    return;
                }
                EVeinType newType = (EVeinType)newValue + 1;
                PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex];
                if (veinGroup.type == newType)
                {
                    return;
                }
                Gardener.VeinGroup.ChangeType(_veinGardenerState.veinGroupIndex, newType, _veinGardenerState.localPlanet);
                int extingProductId = Gardener.VeinGroup.GetProductType(_veinGardenerState.veinGroupIndex, _veinGardenerState.localPlanet);
                UIVGGroupEdit.ProductTypeDropdown.value = _veinGardenerState.products.FindIndex(p => p == extingProductId);
            };

            UIVGGroupEdit.UIOnProductTypeChanged = newValue => {
                if (IsNotValidVGState)
                {
                    return;
                }
                PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex];
                int newProductId = _veinGardenerState.products[newValue];
                int extingProductId = Gardener.VeinGroup.GetProductType(_veinGardenerState.veinGroupIndex, _veinGardenerState.localPlanet);
                if (newProductId == extingProductId)
                {
                    return;
                }

                Debug.Log("Changing product type for vein: " + _veinGardenerState.veinGroupIndex + "(" + veinGroup.type + ") to " + _veinGardenerState.products[newValue]);
                Gardener.VeinGroup.ChangeType(_veinGardenerState.veinGroupIndex, veinGroup.type, _veinGardenerState.localPlanet, productId: newProductId);
            };



            UIVGGroupEdit.UIOnVeinAmountSliderValueChanged = newValue => {
                Debug.Log("VG.GroupEdit : VeinAmountSliderValueChanged : " + newValue);
                Gardener.VeinGroup.UpdateVeinAmount(_veinGardenerState.veinGroupIndex, (long)(newValue / _veinGardenerState.oreMultiplier), _veinGardenerState.localPlanet);

                PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex];
                if (veinGroup.type == EVeinType.Oil)
                {
                    Gardener.VeinGroup.UpdateVeinAmount(_veinGardenerState.veinGroupIndex, (long)(newValue / VeinData.oilSpeedMultiplier), _veinGardenerState.localPlanet);
                }
                else
                {
                    Gardener.VeinGroup.UpdateVeinAmount(_veinGardenerState.veinGroupIndex, (long)(newValue / oreMultiplier), _veinGardenerState.localPlanet);
                }
            };

            //UnityEngine.UI.Dropdown VeinTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOVeinGroupModify, "VeinTypeDropdown");
            //UnityEngine.UI.Dropdown ProductTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOVeinGroupModify, "ProductTypeDropdown");
            //UnityEngine.UI.Dropdown ModeVeinTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOModeSelect, "VeinTypeDropdown");


            Debug.Log("Populating UIVGGroupEdit.VeinTypeDropdown");
            UIVGGroupEdit.VeinTypeDropdown.ClearOptions();

            foreach (EVeinType tabType in Enum.GetValues(typeof(EVeinType)))
            {
                if (tabType == EVeinType.Max || tabType == EVeinType.None)
                {
                    continue;
                }
                int veinProduct = PlanetModelingManager.veinProducts[(int)tabType];
                ItemProto itemProto = veinProduct != 0 ? LDB.items.Select(veinProduct) : null;
                if (itemProto != null)
                {
                    UIVGGroupEdit.VeinTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                }
            }

            Debug.Log("Populating Group Edit Products");
            UIVGGroupEdit.ProductTypeDropdown.ClearOptions();

            foreach (int prodId in _veinGardenerState.products)
            {
                ItemProto itemProto = prodId != 0 ? LDB.items.Select(prodId) : null;
                if (itemProto != null)
                {
                    UIVGGroupEdit.ProductTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                }
            }
            UIVGGroupEdit.ContentTrans = GUIGOVeinGroupModify.GetComponent<RectTransform>();

            UIVGGroupEdit.Hide();
        }

        internal void OnDestroy()
        {
            if (GUIGOVeinGroupModify != null)
            {
                //TestGUI.SetActive(false);
                GameObject.Destroy(GUIGOVeinGroupModify);
            }
        }

        public void Show()
        {
            PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex];
            Debug.Log("Showing vein Group: " + _veinGardenerState.veinGroupIndex);
            int prodId = Gardener.VeinGroup.GetProductType(_veinGardenerState.veinGroupIndex, _veinGardenerState.localPlanet);
            var prodIndex = _veinGardenerState.products.FindIndex(p => p == prodId);

            UIVGGroupEdit.VeinTypeDropdown.value = (int)veinGroup.type - 1;
            UIVGGroupEdit.ProductTypeDropdown.value = prodIndex;

            UIVGGroupEdit.Show();
        }

        public void Hide()
        {
            UIVGGroupEdit.Hide();
        }

        public void Update()
        {
            if (IsNotValidVGState)
            {
                return;
            }
            
            UpdatePos();
            UpdateValues();
        }

        public void UpdateValues()
        {
            PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex]; 
            float currentValue = veinGroup.amount;
            float maxAmount = veinGroup.type == EVeinType.Oil ? oilMax : Math.Min(((long)int.MaxValue - 22845704) * oreMultiplier * veinGroup.count, 25000); // maxAmount = 10;
            float currentMultiplier = veinGroup.type == EVeinType.Oil ? VeinData.oilSpeedMultiplier : oreMultiplier;
            currentValue *= currentMultiplier;
            string currentTextValue = veinGroup.type == EVeinType.Oil ? currentValue.ToString("n1") : veinGroup.amount.ToString("n0");

            UIVGGroupEdit.VeinAmountSlider.maxValue = maxAmount;
            UIVGGroupEdit.UpdateData(UIVGGroupEdit.VeinTypeDropdown.value, UIVGGroupEdit.ProductTypeDropdown.value, veinGroup.count, currentValue, currentTextValue);
        }

        public void UpdatePos()
        {
            if (IsNotValidVGState)
            {
                return;
            }

            PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[_veinGardenerState.veinGroupIndex];
            Vector3 veinGroupWorldPos = veinGroup.pos.normalized * (_veinGardenerState.localPlanet.realRadius + 2.5f);
            Vector3 cameraVeinGroupDistance = veinGroupWorldPos - GameCamera.main.transform.localPosition;
            float num = Vector3.Dot(GameCamera.main.transform.forward, cameraVeinGroupDistance);
            if (cameraVeinGroupDistance.magnitude < 1f || num < 1f || cameraVeinGroupDistance.magnitude > 150)
            {
                Hide();
                return;
            }

            bool flag = UIRoot.ScreenPointIntoRect(GameCamera.main.WorldToScreenPoint(veinGroupWorldPos), ContentParent, out Vector2 rectPoint);
            if (Mathf.Abs(rectPoint.x) > 4000f || Mathf.Abs(rectPoint.y) > 4000f)
            {
                Hide();
            }

            rectPoint.x += UIVGGroupEdit.ContentTrans.rect.width / 2 + 10;
            rectPoint.y += UIVGGroupEdit.ContentTrans.rect.height / 2 + 10;

            rectPoint.x = Mathf.Round(rectPoint.x);
            rectPoint.y = Mathf.Round(rectPoint.y);

            UIVGGroupEdit.ContentTrans.anchoredPosition = rectPoint;
        }
    }
}
