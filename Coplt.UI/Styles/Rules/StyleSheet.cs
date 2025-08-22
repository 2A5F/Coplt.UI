using System.Runtime.CompilerServices;
using Coplt.Mathematics;
using Coplt.UI.Collections;

namespace Coplt.UI.Styles.Rules;

internal struct StyleSheet
{
    #region Fields

    private EmbedMap<StylePropertyId, AnyStyleValue> m_values;
    private EmbedMap<StylePropertyId, Color> m_colors;
    private bool m_has_background_mage;
    private UIImage m_background_mage;
    private bool m_has_box_shadows;
    private BoxShadow m_box_shadows;
    private bool m_has_back_drop;
    private FilterFunc m_back_drop;
    private bool m_has_filter;
    private FilterFunc m_filter;

    #endregion

    #region Set

    #region SetBool

    public void SetBool(StylePropertyId id, bool value)
    {
        if (!id.IsBool()) throw new InvalidOperationException($"{id} is not byte value");
        m_values[id] = AnyStyleValue.MakeBool(value);
    }

    #endregion

    #region SetByte

    public void SetByteEnum<E>(StylePropertyId id, E value) where E : struct, Enum
    {
        if (Unsafe.SizeOf<E>() != 1) throw new InvalidOperationException("sizeof(E) must be 1");
        SetByte(id, Unsafe.BitCast<E, byte>(value));
    }

    public void SetByte(StylePropertyId id, byte value)
    {
        if (!id.IsByte()) throw new InvalidOperationException($"{id} is not byte value");
        m_values[id] = AnyStyleValue.MakeByte(value);
    }

    #endregion

    #region SetInt

    public void SetIntEnum<E>(StylePropertyId id, E value) where E : struct, Enum
    {
        if (Unsafe.SizeOf<E>() != 4) throw new InvalidOperationException("sizeof(E) must be 4");
        SetInt(id, Unsafe.BitCast<E, int>(value));
    }

    public void SetInt(StylePropertyId id, int value)
    {
        if (!id.IsInt()) throw new InvalidOperationException($"{id} is not int value");
        m_values[id] = AnyStyleValue.MakeInt(value);
    }

    #endregion

    #region SetLengthPercentageAuto

    public void SetLengthPercentageAuto(StylePropertyId id, LengthPercentageAuto value)
    {
        if (!id.IsLengthPercentageAuto()) throw new InvalidOperationException($"{id} is not LengthPercentageAuto value");
        m_values[id] = value;
    }

    #endregion

    #region SetDimension

    public void SetDimension(StylePropertyId id, Dimension value)
    {
        if (!id.IsDimension()) throw new InvalidOperationException($"{id} is not Dimension value");
        m_values[id] = value;
    }

    #endregion

    #region SetLengthPercentage

    public void SetLengthPercentage(StylePropertyId id, LengthPercentage value)
    {
        if (!id.IsLengthPercentage()) throw new InvalidOperationException($"{id} is not LengthPercentage value");
        m_values[id] = value;
    }

    #endregion

    #region SetLength

    public void SetLength(StylePropertyId id, Length value)
    {
        if (!id.IsLength()) throw new InvalidOperationException($"{id} is not Length value");
        m_values[id] = value;
    }

    #endregion

    #region SetFloat

    public void SetFloat(StylePropertyId id, float value)
    {
        if (!id.IsFloat()) throw new InvalidOperationException($"{id} is not float value");
        m_values[id] = value;
    }

    public void SetFloat(StylePropertyId id, float? value)
    {
        if (!id.IsFloat()) throw new InvalidOperationException($"{id} is not float value");
        m_values[id] = value;
    }

    #endregion

    #region SetColor

    public void SetColor(StylePropertyId id, Color value)
    {
        if (!id.IsColor()) throw new InvalidOperationException($"{id} is not Color value");
        m_colors[id] = value;
    }

    #endregion

    #region SetImage

    public void SetImage(StylePropertyId id, UIImage value)
    {
        switch (id)
        {
            case StylePropertyId.BackgroundImage:
            {
                m_has_background_mage = true;
                m_background_mage = value;
                break;
            }
            default:
                throw new InvalidOperationException($"{id} is not Image value");
        }
    }

    #endregion

    #region SetBoxShadow

    public void SetBoxShadow(StylePropertyId id, BoxShadow value)
    {
        switch (id)
        {
            case StylePropertyId.BoxShadow:
            {
                m_has_box_shadows = true;
                m_box_shadows = value;
                break;
            }
            default:
                throw new InvalidOperationException($"{id} is not BoxShadow value");
        }
    }

    #endregion

    #region SetImage

    public void SetFilterFunc(StylePropertyId id, FilterFunc value)
    {
        switch (id)
        {
            case StylePropertyId.BackDrop:
            {
                m_has_back_drop = true;
                m_back_drop = value;
                break;
            }
            case StylePropertyId.Filter:
            {
                m_has_filter = true;
                m_filter = value;
                break;
            }
            default:
                throw new InvalidOperationException($"{id} is not FilterFunc value");
        }
    }

    #endregion

    #endregion

    #region Get

    #region TryGetBool

    public bool TryGetBool(StylePropertyId id, out bool value)
    {
        if (!id.IsBool()) throw new InvalidOperationException($"{id} is not byte value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Bool;
            return true;
        }
        value = false;
        return false;
    }

    #endregion

    #region TryGetByte

    public bool TryGetByteEnum<E>(StylePropertyId id, out E value) where E : struct, Enum
    {
        if (Unsafe.SizeOf<E>() != 1) throw new InvalidOperationException("sizeof(E) must be 1");
        var r = TryGetByte(id, out var v);
        value = Unsafe.BitCast<byte, E>(v);
        return r;
    }

    public bool TryGetByte(StylePropertyId id, out byte value)
    {
        if (!id.IsByte()) throw new InvalidOperationException($"{id} is not byte value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Byte;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region TryGetInt

    public bool TryGetIntEnum<E>(StylePropertyId id, out E value) where E : struct, Enum
    {
        if (Unsafe.SizeOf<E>() != 4) throw new InvalidOperationException("sizeof(E) must be 4");
        var r = TryGetByte(id, out var v);
        value = Unsafe.BitCast<int, E>(v);
        return r;
    }

    public bool TryGetInt(StylePropertyId id, out int value)
    {
        if (!id.IsInt()) throw new InvalidOperationException($"{id} is not int value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Int;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region TryGetLengthPercentageAuto

    public bool TryGetLengthPercentageAuto(StylePropertyId id, out LengthPercentageAuto value)
    {
        if (!id.IsLengthPercentageAuto()) throw new InvalidOperationException($"{id} is not LengthPercentageAuto value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Tag switch
            {
                AnyStyleValue.Tags.Auto => LengthPercentageAuto.MakeAuto(),
                AnyStyleValue.Tags.Fixed => LengthPercentageAuto.MakeFixed(v.Fixed),
                AnyStyleValue.Tags.Percent => LengthPercentageAuto.MakePercent(v.Percent),
                AnyStyleValue.Tags.Calc => LengthPercentageAuto.MakeCalc(v.Calc),
                _ => throw new ArgumentOutOfRangeException()
            };
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region TryGetDimension

    public bool TryGetDimension(StylePropertyId id, out Dimension value)
    {
        if (!id.IsDimension()) throw new InvalidOperationException($"{id} is not Dimension value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Tag switch
            {
                AnyStyleValue.Tags.Auto => Dimension.MakeAuto(),
                AnyStyleValue.Tags.Fixed => Dimension.MakeFixed(v.Fixed),
                AnyStyleValue.Tags.Percent => Dimension.MakePercent(v.Percent),
                AnyStyleValue.Tags.Calc => Dimension.MakeCalc(v.Calc),
                _ => throw new ArgumentOutOfRangeException()
            };
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region TryGetLengthPercentage

    public bool TryGetLengthPercentage(StylePropertyId id, out LengthPercentage value)
    {
        if (!id.IsLengthPercentage()) throw new InvalidOperationException($"{id} is not LengthPercentage value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Tag switch
            {
                AnyStyleValue.Tags.Fixed => LengthPercentage.MakeFixed(v.Fixed),
                AnyStyleValue.Tags.Percent => LengthPercentage.MakePercent(v.Percent),
                AnyStyleValue.Tags.Calc => LengthPercentage.MakeCalc(v.Calc),
                _ => throw new ArgumentOutOfRangeException()
            };
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region TryGetLength

    public bool TryGetLength(StylePropertyId id, out Length value)
    {
        if (!id.IsLength()) throw new InvalidOperationException($"{id} is not Length value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.Tag switch
            {
                AnyStyleValue.Tags.Fixed => Length.MakeFixed(v.Fixed),
                AnyStyleValue.Tags.Calc => Length.MakeCalc(v.Calc),
                _ => throw new ArgumentOutOfRangeException()
            };
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region TryGetFloat

    public bool TryGetFloat(StylePropertyId id, out float? value)
    {
        if (!id.IsFloat()) throw new InvalidOperationException($"{id} is not float value");
        if (m_values.TryGet(id, out var v))
        {
            value = v.IsNone ? null : v.Fixed;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region TryGetColor

    public bool TryGetColor(StylePropertyId id, out Color value)
    {
        if (!id.IsColor()) throw new InvalidOperationException($"{id} is not Color value");
        if (m_colors.TryGet(id, out var v))
        {
            value = v;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region TryGetImage

    public bool TryGetImage(StylePropertyId id, out UIImage value)
    {
        switch (id)
        {
            case StylePropertyId.BackgroundImage:
                value = m_background_mage;
                return m_has_background_mage;
            default:
                throw new InvalidOperationException($"{id} is not Image value");
        }
    }

    #endregion

    #region TryGetBoxShadow

    public bool TryGetBoxShadow(StylePropertyId id, out BoxShadow value)
    {
        switch (id)
        {
            case StylePropertyId.BoxShadow:
                value = m_box_shadows;
                return m_has_box_shadows;
            default:
                throw new InvalidOperationException($"{id} is not BoxShadow value");
        }
    }

    #endregion

    #region TryGetFilterFunc

    public bool TryGetFilterFunc(StylePropertyId id, out FilterFunc value)
    {
        switch (id)
        {
            case StylePropertyId.BackDrop:
                value = m_back_drop;
                return m_has_back_drop;
            case StylePropertyId.Filter:
                value = m_filter;
                return m_has_filter;
            default:
                throw new InvalidOperationException($"{id} is not FilterFunc value");
        }
    }

    #endregion

    #endregion

    #region Unset

    public void Unset(StylePropertyId id)
    {
        switch (id)
        {
            case StylePropertyId.BackgroundImage:
                m_has_background_mage = false;
                m_background_mage = default;
                break;
            case StylePropertyId.BoxShadow:
                m_has_box_shadows = false;
                m_box_shadows = default;
                break;
            case StylePropertyId.BackDrop:
                m_has_back_drop = false;
                m_back_drop = default;
                break;
            case StylePropertyId.Filter:
                m_has_filter = false;
                m_filter = default;
                break;
            case StylePropertyId.BackgroundColor:
            case StylePropertyId.BackgroundImageTint:
            case StylePropertyId.BorderColorTop:
            case StylePropertyId.BorderColorRight:
            case StylePropertyId.BorderColorBottom:
            case StylePropertyId.BorderColorLeft:
            case StylePropertyId.TextColor:
                m_colors.Remove(id);
                break;
            default:
                m_values.Remove(id);
                break;
        }
    }

    #endregion
}
