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
import { Route as ManufacturerDetailRouteImport } from './manufacturers.$id'
import { $api } from '@/lib/api/client'

vi.mock('@/lib/api/client', () => ({
  $api: { useQuery: vi.fn() },
}))

const mockUseQuery = vi.mocked($api.useQuery)

const manufacturerDetail = {
  id: 'mfr-1',
  name: 'Corning',
  websiteUrl: 'https://corning.com',
  hasThumbnail: false,
  plates: [
    { id: 'plate-1', name: 'Corning 96-Well', catalogNumber: 'CLS3595', wellCount: 96 },
    { id: 'plate-2', name: 'Corning 384-Well', catalogNumber: 'CLS3701', wellCount: 384 },
  ],
}

function renderAtPath(path: string) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const rootRoute = createRootRoute({ component: Outlet })

  const route = ManufacturerDetailRouteImport.update({
    getParentRoute: () => rootRoute,
    id: '/manufacturers/$id',
    path: '/manufacturers/$id',
  } as Parameters<typeof ManufacturerDetailRouteImport.update>[0])

  const router = createRouter({
    routeTree: rootRoute.addChildren([route as typeof ManufacturerDetailRouteImport]),
    history: createMemoryHistory({ initialEntries: [path] }),
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

describe('ManufacturerDetailPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows loading state while fetching', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    expect(await screen.findByText('Loading…')).toBeTruthy()
  })

  it('renders manufacturer name and website', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturerDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    expect(await screen.findByText('Corning')).toBeTruthy()
    expect(screen.getByText('corning.com')).toBeTruthy()
  })

  it('renders plate count stat', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturerDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    expect(await screen.findByText('2')).toBeTruthy()
  })

  it('renders plates in the table', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturerDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    expect(await screen.findByText('Corning 96-Well')).toBeTruthy()
    expect(screen.getByText('CLS3595')).toBeTruthy()
    expect(screen.getByText('Corning 384-Well')).toBeTruthy()
  })

  it('shows not found message on error', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    expect(await screen.findByText('Manufacturer not found.')).toBeTruthy()
  })

  it('calls useQuery with the manufacturer id from the URL', async () => {
    mockUseQuery.mockReturnValue({
      data: manufacturerDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/manufacturers/mfr-1')
    await screen.findByText('Corning')

    expect(mockUseQuery).toHaveBeenCalledWith(
      'get',
      '/api/manufacturers/{id}',
      expect.objectContaining({
        params: { path: { id: 'mfr-1' } },
      }),
    )
  })
})
