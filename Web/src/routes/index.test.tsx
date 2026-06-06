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
import { Route as IndexRouteImport } from './index'
import { $api } from '@/lib/api/client'
import type { components } from '@/lib/api/schema.gen'

vi.mock('@/lib/api/client', () => ({
  $api: { useQuery: vi.fn() },
}))

const mockUseQuery = vi.mocked($api.useQuery)

const plate: components['schemas']['PlateResponse'] = {
  id: 'aaaa-0000',
  name: 'Corning 96-Well',
  catalogNumber: 'CLS3595',
  wellCount: 96,
  material: { code: 'PS', name: 'Polystyrene' },
  manufacturerId: 'mfr-1',
  manufacturerName: 'Corning',
  properties: [],
}

const pagedResult: components['schemas']['PagedResultOfPlateResponse'] = {
  items: [plate],
  totalCount: 1,
  pageIndex: 0,
  pageSize: 25,
}

function renderAtPath(path = '/') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const rootRoute = createRootRoute({ component: Outlet })

  const indexRoute = IndexRouteImport.update({
    getParentRoute: () => rootRoute,
    id: '/',
    path: '/',
  } as Parameters<typeof IndexRouteImport.update>[0])

  const router = createRouter({
    routeTree: rootRoute.addChildren([indexRoute as typeof IndexRouteImport]),
    history: createMemoryHistory({ initialEntries: [path] }),
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

describe('PlatesPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows loading state while fetching', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByText('Loading…')).toBeTruthy()
  })

  it('renders plate rows from API data', async () => {
    mockUseQuery.mockReturnValue({
      data: pagedResult,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByText('Corning 96-Well')).toBeTruthy()
    expect(screen.getByText('CLS3595')).toBeTruthy()
    expect(screen.getByText('Corning')).toBeTruthy()
  })

  it('shows empty state when no plates are returned', async () => {
    mockUseQuery.mockReturnValue({
      data: { ...pagedResult, items: [], totalCount: 0 },
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByText('No plates found')).toBeTruthy()
  })

  it('renders the search input', async () => {
    mockUseQuery.mockReturnValue({
      data: pagedResult,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/')
    expect(await screen.findByRole('searchbox')).toBeTruthy()
  })

  it('passes search query param to $api.useQuery', async () => {
    mockUseQuery.mockReturnValue({
      data: pagedResult,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/?search=corning')
    await screen.findByText('Corning 96-Well')

    expect(mockUseQuery).toHaveBeenCalledWith(
      'get',
      '/api/plates',
      expect.objectContaining({
        params: expect.objectContaining({
          query: expect.objectContaining({ search: 'corning' }),
        }),
      }),
    )
  })
})
