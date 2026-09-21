// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

import OriginalHtmlNavbarItem from '@theme-original/NavbarItem/HtmlNavbarItem'
import type { Props } from '@theme/NavbarItem/HtmlNavbarItem'
import React from 'react'
import LanguageSwitch from '../../components/LanguageSwitch'

export default function HtmlNavbarItem(props: Props) {
  if (props.className === 'hyprism-language-switch-slot') {
    return (
      <div className={`${props.mobile ? 'menu__list-item' : 'navbar__item'} hyprism-language-switch-slot`}>
        <LanguageSwitch />
      </div>
    )
  }
  return <OriginalHtmlNavbarItem {...props} />
}
