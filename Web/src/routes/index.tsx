import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { z } from "zod";
import { useState, useEffect } from "react";
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  createColumnHelper,
} from "@tanstack/react-table";
import { $api } from "@/lib/api/client";
import type { components } from "@/lib/api/schema.gen";

type PlateRow = components["schemas"]["PlateResponse"];

const searchSchema = z.object({
  search: z.string().optional().default(""),
  pageIndex: z.number().int().min(0).optional().default(0),
  pageSize: z.number().int().min(1).max(100).optional().default(25),
});

export const Route = createFileRoute("/")({
  validateSearch: searchSchema,
  component: PlatesPage,
});

const columnHelper = createColumnHelper<PlateRow>();

const columns = [
  columnHelper.accessor("name", {
    header: "Name",
  }),
  columnHelper.accessor("catalogNumber", {
    header: "Catalog No.",
  }),
  columnHelper.accessor("wellCount", {
    header: "Wells",
  }),
  columnHelper.accessor("material", {
    header: "Material",
    cell: (info) => `${info.getValue().code} — ${info.getValue().name}`,
  }),
  columnHelper.accessor("manufacturerName", {
    header: "Manufacturer",
  }),
];

function PlatesPage() {
  const navigate = useNavigate({ from: "/" });
  const { search, pageIndex, pageSize } = Route.useSearch();

  const [inputValue, setInputValue] = useState(search);

  // Sync input when URL param changes externally (e.g. back navigation)
  useEffect(() => {
    setInputValue(search);
  }, [search]);

  // Debounce search input → URL param
  useEffect(() => {
    const timer = setTimeout(() => {
      if (inputValue !== search) {
        void navigate({
          search: (prev) => ({ ...prev, search: inputValue, pageIndex: 0 }),
        });
      }
    }, 300);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [inputValue]);

  const { data, isLoading } = $api.useQuery("get", "/api/plates", {
    params: {
      query: {
        search: search || undefined,
        pageIndex,
        pageSize,
      },
    },
  });

  const totalCount = data?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / pageSize);
  const canPrev = pageIndex > 0;
  const canNext = pageIndex < totalPages - 1;

  const table = useReactTable({
    data: data?.items ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    manualFiltering: true,
    rowCount: totalCount,
    state: {
      pagination: { pageIndex, pageSize },
    },
  });

  const goTo = (idx: number) =>
    void navigate({ search: (prev) => ({ ...prev, pageIndex: idx }) });

  return (
    <main className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-1">Plates</h1>
        <p className="text-gray-500 text-sm">
          Search and browse microwell plates
        </p>
      </div>

      <div className="mb-4">
        <input
          type="search"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="Search by name or catalog number…"
          aria-label="Search plates"
          className="w-full max-w-md px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="border border-gray-200 rounded-lg overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            {table.getHeaderGroups().map((hg) => (
              <tr key={hg.id}>
                {hg.headers.map((header) => (
                  <th
                    key={header.id}
                    className="px-4 py-3 text-left font-medium text-gray-600 uppercase tracking-wide text-xs"
                  >
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext(),
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-gray-100 bg-white">
            {isLoading ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-10 text-center text-gray-400"
                >
                  Loading…
                </td>
              </tr>
            ) : table.getRowModel().rows.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-10 text-center text-gray-400"
                >
                  No plates found
                </td>
              </tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr
                  key={row.id}
                  role="button"
                  tabIndex={0}
                  onClick={() =>
                    void navigate({
                      to: "/plates/$id",
                      params: { id: row.original.id },
                    })
                  }
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ")
                      void navigate({
                        to: "/plates/$id",
                        params: { id: row.original.id },
                      });
                  }}
                  className="hover:bg-blue-50 cursor-pointer transition-colors"
                >
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-3 text-gray-800">
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="mt-4 flex items-center justify-between text-sm text-gray-600">
        <span>
          {totalCount > 0
            ? `${pageIndex * pageSize + 1}–${Math.min((pageIndex + 1) * pageSize, totalCount)} of ${totalCount}`
            : ""}
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => goTo(pageIndex - 1)}
            disabled={!canPrev}
            className="px-3 py-1 border border-gray-300 rounded disabled:opacity-40 hover:bg-gray-50 disabled:cursor-not-allowed"
          >
            Previous
          </button>
          <button
            onClick={() => goTo(pageIndex + 1)}
            disabled={!canNext}
            className="px-3 py-1 border border-gray-300 rounded disabled:opacity-40 hover:bg-gray-50 disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      </div>
    </main>
  );
}
