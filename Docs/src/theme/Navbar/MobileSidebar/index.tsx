// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

import {
  useLockBodyScroll,
  useNavbarMobileSidebar,
} from '@docusaurus/theme-common/internal'
import { useLocation } from '@docusaurus/router'
import OriginalPrimaryMenu from '@theme-original/Navbar/MobileSidebar/PrimaryMenu'
import SearchBar from '@theme/SearchBar'
import React, { useEffect, useRef } from 'react'
import { DocsSidebarNavigation } from '../../../components/DocsSidebar'

export default function NavbarMobileSidebar() {
  const { shouldRender, shown, toggle } = useNavbarMobileSidebar()
  const location = useLocation()
  const panelRef = useRef<HTMLDivElement>(null)

  useLockBodyScroll(shown)

  useEffect(() => {
    if (shown) {
      toggle()
    }
  }, [location.pathname])

  useEffect(() => {
    if (!shown) {
      return
    }

    const dismiss = (event: PointerEvent) => {
      const target = event.target
      if (
        panelRef.current?.contains(target as Node) ||
        (target instanceof Element && target.closest('.navbar__toggle'))
      ) {
        return
      }
      toggle()
    }
    const escape = (event: KeyboardEvent) => {
      if (event.key !== 'Escape') {
        return
      }
      toggle()
      document.querySelector<HTMLButtonElement>('.navbar__toggle')?.focus()
    }

    document.addEventListener('pointerdown', dismiss)
    document.addEventListener('keydown', escape)

    return () => {
      document.removeEventListener('pointerdown', dismiss)
      document.removeEventListener('keydown', escape)
    }
  }, [shown, toggle])

  if (!shouldRender) {
    return null
  }

  return (
    <div
      ref={panelRef}
      className="hyprism-navbar-docs-panel"
      data-open={shown}
      aria-hidden={!shown}
      inert={!shown}
    >
      <div className="hyprism-navbar-docs-search">
        <SearchBar />
      </div>
      <DocsSidebarNavigation onNavigate={() => shown && toggle()} />
      <OriginalPrimaryMenu />
    </div>
  )
}
