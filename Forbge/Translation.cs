using System;
using System.Collections.Generic;
using I2.Loc;

namespace Forbge;

public class Translation {
    private string _default = null;
    private bool _defaultSet = false;
    private Dictionary<string, string> _localised = new Dictionary<string, string>();
    private TermData _termData = null;

    public string Default {
        get => _default;
        set {
            _default = value;
            _defaultSet = true;
            if (_termData != null)
                UpdateTermData();
        }
    }
    public string this[string code] {
        get => _localised[code];
        set {
            _localised[code] = value;
            if (!_defaultSet) {
                _default = value;
                _defaultSet = true;
            }
            if (_termData != null)
                UpdateTermData();
        }
    }

    internal Translation(string initDefault = null) {
        _default = initDefault;
    }

    internal void Register(string locKey) {
        LocalizationManager.InitializeIfNeeded();
        _termData = LocalizationManager.Sources[0].AddTerm(locKey);
        UpdateTermData();
    }
    internal void UpdateTermData() {
        _termData.Description = _default;
        var langdata = LocalizationManager.Sources[0].mLanguages;
        for (int i = 0; i < langdata.Count; i++) {
            _termData.Languages[i] = _localised.GetValueOrDefault(langdata[i].Code, _default);
            // The only flag seems to be: Flags[i] & 2 == auto-translated
            _termData.Flags[i] = 0;
        }
    }

    internal void Clone(Translation other) {
        _default = other._default;
        _defaultSet = other._defaultSet;
        _localised.Clear();
        foreach (var i in other._localised)
            _localised[i.Key] = i.Value;
        if (_termData != null)
            UpdateTermData();
    }
}
