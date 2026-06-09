import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import {
  createMemoryHistory,
  createRootRoute,
  createRouter,
  RouterProvider,
  Outlet,
} from '@tanstack/react-router'
import { Route as SearchRouteImport } from './index'
import { $api } from '@/lib/api/client'
import type { components } from '@/lib/api/schema.gen'

vi.mock('@/lib/api/client', () => ({
  $api: { useQuery: vi.fn() },
}))

const mockUseQuery = vi.mocked($api.useQuery)

const plateResult: components['schemas']['SearchResultItem'] = {
  entityType: 'Plate',
  id: 'plate-001',
  name: 'Corning 96-Well',
  hasThumbnail: false,
  catalogNumber: 'CLS3595',
  wellCount: 96,
  manufacturerId: 'mfr-1',
  manufacturerName: 'Corning',
  websiteUrl: null,
}

const manufacturerResult: components['schemas']['SearchResultItem'] = {
  entityType: 'Manufacturer',
  id: 'mfr-1',
  name: 'Corning',
  hasThumbnail: false,
  catalogNumber: null,
  wellCount: null,
  manufacturerId: null,
  manufacturerName: null,
  websiteUrl: 'https://corning.com',
}

const searchResponse: components['schemas']['SearchResponse'] = {
  items: [plateResult, manufacturerResult],
  totalCount: 2,
}

function renderAtPath(path = '/') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const rootRoute = createRootRoute({ component: Outlet })

  const searchRoute = SearchRouteImport.update({
    getParentRoute: () => rootRoute,
    id: '/',
    path: '/',
  } as Parameters<typeof SearchRouteImport.update>[0])

  const router = createRouter({
    routeTree: rootRoute.addChildren([searchRoute as typeof SearchRouteImport]),
    history: createMemoryHistory({ initialEntries: [path] }),
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

describe('SearchPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('renders the search input', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByRole('searchbox')).toBeTruthy()
  })

  it('shows hero heading', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByText(/find any plate or manufacturer/i)).toBeTruthy()
  })

  it('shows searching state while loading', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/?q=corning')
    expect(await screen.findByText('Searching…')).toBeTruthy()
  })

  it('shows no results message when search returns empty', async () => {
    mockUseQuery.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/?q=notfound')
    expect(await screen.findByText(/"notfound"/i)).toBeTruthy()
  })

  it('renders plate and manufacturer search results', async () => {
    mockUseQuery.mockReturnValue({
      data: searchResponse,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/?q=corning')
    expect(await screen.findByText('Corning 96-Well')).toBeTruthy()
    expect(screen.getAllByText('Corning').length).toBeGreaterThan(0)
  })

  it('calls useQuery with the q param from the URL', async () => {
    mockUseQuery.mockReturnValue({
      data: searchResponse,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/?q=corning')
    await screen.findByText('Corning 96-Well')

    expect(mockUseQuery).toHaveBeenCalledWith(
      'get',
      '/api/search',
      expect.objectContaining({
        params: expect.objectContaining({
          query: expect.objectContaining({ q: 'corning' }),
        }),
      }),
      expect.anything(),
    )
  })
})

