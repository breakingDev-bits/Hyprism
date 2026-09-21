// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

import Link from '@docusaurus/Link'
import { useLocation } from '@docusaurus/router'
import useBaseUrl from '@docusaurus/useBaseUrl'
import clsx from 'clsx'
import React from 'react'
import { useDocsLocale } from '../context/locale'
import { routeToUrl, useLocalizedDocsData, type NavigationItem } from '../data'
import { dictionaries } from '../i18n'
import { MaterialSymbol } from './MaterialSymbol'

function containsRoute(item: NavigationItem, activeRoute: string): boolean {
  return item.type === 'link'
    ? item.route === activeRoute
    : item.items.some(child => containsRoute(child, activeRoute))
}

function SidebarItem({ item, activeRoute, onNavigate }: Readonly<{
  item: NavigationItem
  activeRoute: string
  onNavigate?: () => void
}>) {
  if (item.type === 'link') {
    const active = item.route === activeRoute
    return (
      <li>
        <Link
          className={clsx('hyprism-sidebar-link', active && 'is-active')}
          aria-current={active ? 'page' : undefined}
          to={routeToUrl(item.route)}
          onClick={onNavigate}
        >
          {item.label}
        </Link>
      </li>
    )
  }

  return (
    <li>
      <details className="hyprism-sidebar-category" open={containsRoute(item, activeRoute)}>
        <summary>
          {item.icon && (
            <MaterialSymbol name={item.icon} size={17} className="hyprism-sidebar-icon" />
          )}
          {item.label}
        </summary>
        <ul>
          {item.items.map(child => (
            <SidebarItem
              key={child.type === 'link' ? child.route : child.label}
              item={child}
              activeRoute={activeRoute}
              onNavigate={onNavigate}
            />
          ))}
        </ul>
      </details>
    </li>
  )
}

export function DocsSidebarNavigation({ onNavigate }: Readonly<{ onNavigate?: () => void }>) {
  const { locale } = useDocsLocale()
  const { navigation } = useLocalizedDocsData()
  const location = useLocation()
  const docsBaseUrl = useBaseUrl('/').replace(/\/$/, '')
  const activeRoute = location.pathname
    .replace(/\/$/, '')
    .replace(docsBaseUrl, '')
    .replace(/^\//, '')
  const dictionary = dictionaries[locale]

  return (
    <nav className="hyprism-docs-navigation" aria-label={dictionary.menu}>
      <ul>
        {navigation[locale].map(item => (
          <SidebarItem
            key={item.type === 'link' ? item.route : item.label}
            item={item}
            activeRoute={activeRoute}
            onNavigate={onNavigate}
          />
        ))}
      </ul>
    </nav>
  )
}

export default function DocsSidebar() {
  return (
    <aside className="hyprism-docs-sidebar">
      <DocsSidebarNavigation />
    </aside>
  )
}
