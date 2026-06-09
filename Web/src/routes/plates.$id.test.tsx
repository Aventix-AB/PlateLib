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
import { Route as PlateDetailRouteImport } from './plates.$id'
import { $api } from '@/lib/api/client'

vi.mock('@/lib/api/client', () => ({
  $api: { useQuery: vi.fn() },
}))

const mockUseQuery = vi.mocked($api.useQuery)

const plateDetail = {
  id: 'plate-001',
  name: 'Nunc 384-Well',
  catalogNumber: 'P7735',
  wellCount: 384,
  material: { code: 'PS', name: 'Polystyrene' },
  manufacturerId: 'mfr-2',
  manufacturerName: 'Thermo Fisher',
  properties: [
    { name: 'Color', value: 'White' },
    { name: 'Sterility', value: 'Sterile' },
  ],
  files: [
    { id: 'file-001', fileName: 'drawing.pdf', contentType: 'application/pdf' },
  ],
}

function renderAtPath(path: string) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const rootRoute = createRootRoute({ component: Outlet })

  const plateDetailRoute = PlateDetailRouteImport.update({
    getParentRoute: () => rootRoute,
    id: '/plates/$id',
    path: '/plates/$id',
  } as Parameters<typeof PlateDetailRouteImport.update>[0])

  const router = createRouter({
    routeTree: rootRoute.addChildren([plateDetailRoute as typeof PlateDetailRouteImport]),
    history: createMemoryHistory({ initialEntries: [path] }),
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

describe('PlateDetailPage', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows loading state while fetching', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('Loading…')).toBeTruthy()
  })

  it('renders plate name and catalog number', async () => {
    mockUseQuery.mockReturnValue({
      data: plateDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('Nunc 384-Well')).toBeTruthy()
    expect(screen.getByText('P7735')).toBeTruthy()
  })

  it('renders manufacturer, well count, and material', async () => {
    mockUseQuery.mockReturnValue({
      data: plateDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('Thermo Fisher')).toBeTruthy()
    expect(screen.getByText('384')).toBeTruthy()
    expect(screen.getByText('PS — Polystyrene')).toBeTruthy()
  })

  it('renders properties', async () => {
    mockUseQuery.mockReturnValue({
      data: plateDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('White')).toBeTruthy()
    expect(screen.getByText('Sterile')).toBeTruthy()
  })

  it('renders file download link', async () => {
    mockUseQuery.mockReturnValue({
      data: plateDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('drawing.pdf')).toBeTruthy()
  })

  it('shows not found message on error', async () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    expect(await screen.findByText('Plate not found.')).toBeTruthy()
  })

  it('calls useQuery with the plate id from the URL', async () => {
    mockUseQuery.mockReturnValue({
      data: plateDetail,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof mockUseQuery>)

    renderAtPath('/plates/plate-001')
    await screen.findByText('Nunc 384-Well')

    expect(mockUseQuery).toHaveBeenCalledWith(
      'get',
      '/api/plates/{id}',
      expect.objectContaining({
        params: { path: { id: 'plate-001' } },
      }),
    )
  })
})
