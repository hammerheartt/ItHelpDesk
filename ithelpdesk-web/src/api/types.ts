import type { Role } from "../auth/jwt";

export type TicketStatus = 'New' | 'Assigned' | 'InProgress' | 'Resolved' | 'Closed' | 'Reopened' | 'Cancelled'
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export const TICKET_STATUSES: TicketStatus[] = ['New', 'Assigned', 'InProgress', 'Resolved', 'Closed', 'Reopened', 'Cancelled']
export const TICKET_PRIORITIES: TicketPriority[] = ['Low', 'Medium', 'High', 'Critical']

export type Ticket = {
  id: number
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority | null
  categoryId: number
  categoryName: string
  createdById: string
  createdByName: string
  assignedToId: string | null
  assignedToName: string | null
  createdAt: string
  dueAt: string | null
  resolvedAt: string | null
  closedAt: string | null
  reopenCount: number
  resolutionNote: string | null
  isOverdue: boolean
}

export type Category = { id: number; name: string; isActive: boolean }

export type User = { id: string; fullName: string; email: string; role: Role; isActive: boolean }

export type PaginationMetadata = {
  totalCount: number
  pageSize: number
  currentPage: number
  totalPages: number
  previousPageLink: string | null
  nextPageLink: string | null
}

export type Comment = {
    id: number
    text: string
    isInternal: boolean
    authorId: string
    authorName: string
    createdAt: string
}

export type HistoryEntry = {
    id: number
    action: string
    oldValue: string | null
    newValue: string | null
    changedById: string
    changedByName: string
    changedAt: string
}

export type Stats = {
  totalTickets: number
  ticketsByStatus: Record<TicketStatus, number>
  unassignedTickets: number
  openTickets: number
  overdueTickets: number
  reopenedTickets: number
  averageResolutionHours: number | null
  ticketsByCategory: { category: string; count: number }[]
  technicians: {
    id: string
    fullName: string
    isActive: boolean
    openTickets: number
    resolvedTickets: number
  }[]
}