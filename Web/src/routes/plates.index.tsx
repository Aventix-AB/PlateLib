import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { z } from "zod";
import { useState, useEffect } from "react";
import { ExternalLink } from "lucide-react";
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  createColumnHelper,
} from "@tanstack/react-table";
import { $api } from "@/lib/api/client";

interface PlateRow {
  id: string;
  name: string;
  catalogNumber: string;
  productUrl: string;
  wellCount: number;
  material: { code: string; name: string };
  manufacturerId: string;
  manufacturerName: string;
  properties: { name: string; value: string }[];
}

const searchSchema = z.object({
  search: z.string().optional().default(""),
  pageIndex: z.number().int().min(0).optional().default(0),
  pageSize: z.number().int().min(1).max(100).optional().default(25),
});

export const Route = createFileRoute("/plates/")({
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
  columnHelper.accessor("productUrl", {
    header: "Product",
    cell: (info) => (
      <a
        href={info.getValue()}
        target="_blank"
        rel="noopener noreferrer"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={(e) => e.stopPropagation()}
        className="inline-flex items-center gap-1 text-primary hover:underline"
      >
        Open
        <ExternalLink size={13} />
      </a>
    ),
  }),
];

function PlatesPage() {
  const navigate = useNavigate({ from: "/plates" });
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

  const { data: rawData, isLoading } = $api.useQuery("get", "/api/plates", {
    params: {
      query: {
        search: search || undefined,
        pageIndex,
        pageSize,
      },
    },
  });

  const data = rawData as
    | {
        items: PlateRow[];
        totalCount: number;
        pageIndex: number;
        pageSize: number;
      }
    | undefined;

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
    <main className="pl-page">
      <div className="pl-page-header">
        <h1 className="pl-page-title">Plates</h1>
        <p className="pl-page-subtitle">Search and browse microwell plates</p>
      </div>

      <div className="mb-4">
        <input
          type="search"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="Search by name or catalog number…"
          aria-label="Search plates"
          className="pl-input max-w-md"
        />
      </div>

      <div className="pl-table-container">
        <table className="pl-table">
          <thead className="pl-table-head">
            {table.getHeaderGroups().map((hg) => (
              <tr key={hg.id}>
                {hg.headers.map((header) => (
                  <th key={header.id} className="pl-table-th">
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext(),
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody className="pl-table-body">
            {isLoading ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-10 text-center text-muted-foreground"
                >
                  Loading…
                </td>
              </tr>
            ) : table.getRowModel().rows.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-10 text-center text-muted-foreground"
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
                  className="pl-table-row-interactive"
                >
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="pl-table-td">
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

      <div className="pl-pagination">
        <span>
          {totalCount > 0
            ? `${pageIndex * pageSize + 1}–${Math.min((pageIndex + 1) * pageSize, totalCount)} of ${totalCount}`
            : ""}
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => goTo(pageIndex - 1)}
            disabled={!canPrev}
            className="pl-pagination-btn"
          >
            Previous
          </button>
          <button
            onClick={() => goTo(pageIndex + 1)}
            disabled={!canNext}
            className="pl-pagination-btn"
          >
            Next
          </button>
        </div>
      </div>
    </main>
  );
}
