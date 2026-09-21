// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

export const locales = ['en', 'ru'] as const

export type Locale = (typeof locales)[number]

export const dictionaries = {
  en: {
    docsLabel: 'Docs',
    editPage: 'Edit this page on GitHub',
    languageSwitcher: 'Choose documentation language',
    languages: {
      en: 'English',
      ru: 'Russian'
    },
    menu: 'Documentation navigation',
    next: 'Next',
    previous: 'Previous',
    search: {
      label: 'Search documentation',
      placeholder: 'Search docs',
      empty: 'No results found'
    },
    footer: {
      backToTop: 'Back to top',
      repository: 'Source on GitHub',
      teamSite: 'Visit the team site',
      license: 'GPL-3.0-only'
    },
    toc: 'On this page'
  },
  ru: {
    docsLabel: 'Документация',
    editPage: 'Редактировать страницу на GitHub',
    languageSwitcher: 'Выбрать язык документации',
    languages: {
      en: 'Английский',
      ru: 'Русский'
    },
    menu: 'Навигация по документации',
    next: 'Далее',
    previous: 'Назад',
    search: {
      label: 'Поиск по документации',
      placeholder: 'Поиск',
      empty: 'Ничего не найдено'
    },
    footer: {
      backToTop: 'Наверх',
      repository: 'Исходный код на GitHub',
      teamSite: 'Открыть сайт команды',
      license: 'GPL-3.0-only'
    },
    toc: 'На этой странице'
  }
} as const
