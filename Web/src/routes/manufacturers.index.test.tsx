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
import { Route as ManufacturersRouteImport } from './manufacturers.index'
import { $api } from '@/lib/api/client'

vi.mock('@/lib/api/client', () => ({
  $api: { useQuery: vi.fn() },
}))

const mockUseQuery = vi.mocked($api.useQuery)

const manufacturers = [
  { id: 'mfr-1', name: 'Corning', websiteUrl: 'https://corning.com', hasThumbnail: false },
  { id: 'mfr-2', name: 'Thermo Fisher', websiteUrl: null, hasThumbnail: true },
]

function renderAtPath(path = '/manufacturers') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const rootRoute = createRootRoute({ component: Outlet })

  const route = ManufacturersRouteImport.update({
    getParentRoute: () => rootRoute,
    id: '/manufacturers/',
    path: '/manufacturers/',
  } as Parameters<typeof ManufacturersRouteImport.update>[0])

  const router = createRouter({
    routeTree: rootRoute.addChildren([route as typeof ManufacturersRouteImport]),
    history: createMemoryHistory({ initialEntries: [path] }),
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

describe('ManufacturersPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows loading state while fetching', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath()
    expect(await screen.findByText('Loading…')).toBeTruthy()
  })

  it('renders manufacturer cards from API data', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturers,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath()
    expect(await screen.findByText('Corning')).toBeTruthy()
    expect(screen.getByText('Thermo Fisher')).toBeTruthy()
  })

  it('shows website URL for manufacturers that have one', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturers,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath()
    expect(await screen.findByText('corning.com')).toBeTruthy()
  })

  it('shows empty state when no manufacturers are returned', async () => {
    mockUseQuery.mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath()
    expect(await screen.findByText('No manufacturers found.')).toBeTruthy()
  })
})
