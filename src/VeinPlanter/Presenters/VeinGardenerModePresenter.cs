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

    public class VeinGardenerModePresenter : PresenterBase
    {
        public static GameObject GUIGOModeSelect = null;
        public static UIVeinGardenerMode UIVGMode = null;
        private VeinGardenerModel _veinGardenerState;

        public VeinGardenerModePresenter(VeinGardenerModel veinGardenerState)
        {
            _veinGardenerState = veinGardenerState;
        }

        public void Init()
        {
            var inGameWindows = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows");
            //Debug.Log(inGameWindows);

            GUIGOModeSelect = VeinPlanter.Instantiate(VeinPlanter.bundleAssets.LoadAsset<GameObject>("Assets/UI/VeinGardener/UIVeinPlanterMode.prefab"));
            AttachGameObject(GUIGOModeSelect, inGameWindows.transform);

            UIVGMode = GUIGOModeSelect.GetComponent<UIVeinGardenerMode>();
            if (!UIVGMode.IsInit) UIVGMode.Init();

            Debug.Log("Registering callbacks for VG.Mode");
            UIVGMode.UIOnCreateNewVeinGroup = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.AddVeinGroup); Hide(); };
            UIVGMode.UIOnDeleteVein = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.RemoveVein); Hide(); };
            UIVGMode.UIOnDeleteVeinGroup = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.RemoveVeinGroup); Hide(); };
            UIVGMode.UIOnExtendVeinGroup = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.ExtendVein); Hide(); };
            UIVGMode.UIOnModifyVeinGroup = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.ModifyVeinGroup); Hide(); };
            UIVGMode.UIOnWorldEditShowAll = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.PlanetVeins); Hide(); };

            UIVGMode.UIOnCreateNewVeinGroupTypeChanged = newValue => { _veinGardenerState.newVeinGroupType = (EVeinType)(newValue + 1); };

            UIVGMode.UIOnClose = () => { _veinGardenerState.ChangeMode(EVeinModificationMode.Deactivated); Hide(); };

            Debug.Log("Populating UIVGGroupEdit.VeinTypeDropdown");
            UIVGMode.VeinTypeDropdown.ClearOptions();

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
                    UIVGMode.VeinTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                }
            }
            Hide();
        }

        internal void OnDestroy()
        {
            if (GUIGOModeSelect != null)
            {
                GUIGOModeSelect.SetActive(false);
                GameObject.Destroy(GUIGOModeSelect);
            }
        }

        public void Update()
        {

        }

        public void Show()
        {
            VeinGardenerModel.ShowModeMenu = true;
            UIVGMode.Show();
        }

        public void Hide()
        {
            VeinGardenerModel.ShowModeMenu = false;
            UIVGMode.Hide();
        }

        public void HandleClick()
        {
            Vector3 worldPos;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, uicam, out var localPoint);
            Ray ray = GameCamera.main.ScreenPointToRay(Input.mousePosition);
            if (!VFInput.onGUI && Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Clicking screen pos: " + Input.mousePosition.ToString() + " Ray: " + ray.ToString());

                _veinGardenerState.localPlanet = GameMain.localPlanet;
                if (_veinGardenerState.localPlanet == null)
                {
                    return;
                }

                if (Physics.Raycast(ray, out var hitInfo, 1000f, 15873, QueryTriggerInteraction.Ignore))
                {
                    worldPos = hitInfo.point;
                    // Debug.Log("Clicked on world pos: " + worldPos.ToString());

                    Gardener.VeinGroup.GetClosestIndex(ray, _veinGardenerState.localPlanet, out int closestVeinGroupIndex, out int closestVeinIndex, out float closestVeinDistance, out float closestVeinDistance2D);

                    switch (_veinGardenerState.modMode)
                    {
                        case EVeinModificationMode.AddVeinGroup:
                            //if (closestVeinGroupIndex < 0)
                            {
                                var veinGroup = Gardener.VeinGroup.New(_veinGardenerState.newVeinGroupType, worldPos.normalized);
                                closestVeinGroupIndex = _veinGardenerState.localPlanet.AddVeinGroupData(veinGroup);
                                _veinGardenerState.veinGroupIndex = closestVeinGroupIndex;
                                Debug.Log("Adding new veinGroup: " + veinGroup.type.ToString() + " index: " + closestVeinGroupIndex + " Pos: " + veinGroup.pos * _veinGardenerState.localPlanet.radius);
                            }
                            Gardener.Vein.Add(_veinGardenerState.localPlanet, worldPos, closestVeinGroupIndex);
                            break;
                        case EVeinModificationMode.ExtendVein:
                            if (closestVeinDistance2D > 40)
                            {
                                Debug.Log("Not Extending veinGroup.index: " + _veinGardenerState.veinGroupIndex + ". Distance from vein group too large: " + closestVeinDistance2D);
                            }
                            Gardener.Vein.Add(_veinGardenerState.localPlanet, worldPos, _veinGardenerState.veinGroupIndex);
                            break;
                        case EVeinModificationMode.ModifyVeinGroup:
                            if (closestVeinGroupIndex >= 0)
                            {
                                PlanetData.VeinGroup veinGroup = _veinGardenerState.localPlanet.veinGroups[closestVeinGroupIndex];
                                Debug.Log("Clicked on veinGroup: " + veinGroup.ToString() + " index: " + closestVeinGroupIndex + " Type: " + veinGroup.type);
                                Debug.Log("VeinGroup: " + veinGroup.pos.ToString() + " index: " + (veinGroup.pos * (_veinGardenerState.localPlanet.realRadius + 2.5f)));

                                /*
                                dialog = new UIVeinGroupDialog() {
                                    localPlanet = _veinGardenerState.localPlanet,
                                    veinGroupIndex = closestVeinGroupIndex,
                                    Show = true
                                };
                                */
                                _veinGardenerState.veinGroupIndex = closestVeinGroupIndex;
                                VeinPlanter.instance.veinGroupModifyPresenter.Show();
                            }
                            else
                            {
                                VeinPlanter.instance.veinGroupModifyPresenter.Hide();
                            }
                            break;
                        case EVeinModificationMode.RemoveVein:
                            if (closestVeinGroupIndex >= 0 && closestVeinIndex >= 0 && closestVeinDistance2D < 1)
                            {
                                Debug.Log("Removing vein: " + closestVeinIndex + " in group: " + closestVeinGroupIndex);
                                Gardener.Vein.Remove(_veinGardenerState.localPlanet, closestVeinIndex, closestVeinGroupIndex);
                                Gardener.VeinGroup.UpdatePosFromChildren(closestVeinGroupIndex);
                            }
                            break;


                        case EVeinModificationMode.TerrainLower:
                            //TerrainLower(worldPos);
                            break;

                        case EVeinModificationMode.Deactivated:
                        default:
                            break;
                    }
                }
            }
        }
    }
}
