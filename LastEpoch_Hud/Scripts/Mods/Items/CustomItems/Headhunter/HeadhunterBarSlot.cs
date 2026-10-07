using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>One icon entry of the buff bar: framed icon plus countdown digits.</summary>
internal sealed class HeadhunterBarSlot
{
    private static readonly Color _frameColor = new(1f, 0.55f, 0.1f, 1f);
    private static readonly Color _backingColor = new(0.06f, 0.04f, 0.02f, 0.85f);
    private static readonly Color _outlineColor = new(0f, 0f, 0f, 1f);
    private static readonly Vector2 _frameDistance = new(3f, 3f);
    private static readonly SecondsTextCache _secondsTexts = new();

    private readonly GameObject _root;
    private readonly Image _icon;
    private readonly Image _timer;
    private readonly Text _text;
    private int _statId = -1;
    private int _seconds = -1;
    private float _elapsed = -1f;

    public HeadhunterBarSlot(GameObject entry)
    {
        _root = entry;
        GameObject iconPanel = Functions.GetChild(entry, "Panel_Icon");
        _icon = Functions.GetChild(iconPanel, "Icon").GetComponent<Image>();
        _timer = Functions.GetChild(iconPanel, "Timer").GetComponent<Image>();
        _text = Functions.GetChild(iconPanel, "Timer_Text").GetComponent<Text>();
        Functions.GetChild(entry, "Panel_Stack").SetActive(false);
        ApplyFrame(Functions.GetChild(iconPanel, "Background"));
        ApplyTextStyle();
        _timer.gameObject.SetActive(true);
    }

    public void Show(HeadhunterBarEntry entry)
    {
        _root.SetActive(true);
        ApplyIcon(entry.StatId);
        if (entry.SecondsLeft != _seconds)
        {
            _seconds = entry.SecondsLeft;
            _text.text = _secondsTexts.Get(_seconds);
        }

        if (entry.Elapsed != _elapsed)
        {
            _elapsed = entry.Elapsed;
            _timer.fillAmount = _elapsed;
        }
    }

    public void Hide()
    {
        _root.SetActive(false);
    }

    private void ApplyFrame(GameObject background)
    {
        Image image = background.GetComponent<Image>();
        image.sprite = null;
        image.color = _backingColor;
        background.transform.SetAsFirstSibling();
        background.GetComponent<RectTransform>().sizeDelta = _icon
            .GetComponent<RectTransform>()
            .sizeDelta;
        Outline outline = background.AddComponent<Outline>();
        outline.effectColor = _frameColor;
        outline.effectDistance = _frameDistance;
    }

    private void ApplyTextStyle()
    {
        _text.color = Color.white;
        Outline outline = _text.gameObject.AddComponent<Outline>();
        outline.effectColor = _outlineColor;
    }

    private void ApplyIcon(int statId)
    {
        if (statId == _statId && !_icon.sprite.IsNullOrDestroyed())
        {
            return;
        }

        _statId = statId;
        Sprite sprite = HeadhunterBuffIcons.For(statId);
        if (sprite.IsNullOrDestroyed())
        {
            return;
        }

        _icon.overrideSprite = null;
        _icon.sprite = sprite;
    }
}
