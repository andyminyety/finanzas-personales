import api from './axios'
import type { CatalogItem, CatalogSlug } from '../types'

// userId solo lo respeta el servidor si quien llama es administrador (para gestionar los datos de otro usuario).
const q = (userId?: string) => (userId ? { params: { userId } } : undefined)

export const getCatalog = (slug: CatalogSlug, userId?: string) =>
  api.get<CatalogItem[]>(`/catalogs/${slug}`, q(userId)).then((r) => r.data)

export const createCatalogItem = (slug: CatalogSlug, data: { description: string; requiresCard: boolean }, userId?: string) =>
  api.post<CatalogItem>(`/catalogs/${slug}`, data, q(userId)).then((r) => r.data)

export const updateCatalogItem = (slug: CatalogSlug, id: string, data: { description: string; requiresCard: boolean }, userId?: string) =>
  api.put<CatalogItem>(`/catalogs/${slug}/${id}`, data, q(userId)).then((r) => r.data)

export const setCatalogStatus = (slug: CatalogSlug, id: string, isActive: boolean, userId?: string) =>
  api.patch<CatalogItem>(`/catalogs/${slug}/${id}/status`, { isActive }, q(userId)).then((r) => r.data)
