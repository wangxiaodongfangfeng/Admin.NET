import request from '/@/utils/request';

const BASE = '/api/lkCustomStat';

// ─── Types ───────────────────────────────────────────────────────────────────

export interface LkCustomStatDef {
	id: number;
	name: string;
	sqlTemplate: string;
	filterConfig?: string;   // JSON string, e.g. '["date","product_status"]'
	description?: string;
	isEnabled: boolean;
	createTime?: string;
	updateTime?: string;
}

export interface RunStatInput {
	id: number;
	date?: string;           // yyyy-MM-dd — maps to {{date_start}} / {{date_end}}
	month?: string;          // yyyy-MM   — maps to {{month_prefix}}
	productStatusId?: number;
	userId?: number;
}

export interface LkCustomStatResult {
	name: string;
	usedParams: {
		dateStart?: string;
		dateEnd?: string;
		monthPrefix?: string;
		shiftCutoff?: string;
		productStatusId?: number;
		productStatusName?: string;
		userId?: number;
	};
	columns: string[];
	rows: (string | number | null)[][];
}

// ─── API calls ───────────────────────────────────────────────────────────────

/** All enabled stat definitions */
export const listStatDefs = () =>
	request.get<any, { data: { result: LkCustomStatDef[] } }>(`${BASE}/list`);

/** All stat definitions (including disabled) */
export const listAllStatDefs = () =>
	request.get<any, { data: { result: LkCustomStatDef[] } }>(`${BASE}/listAll`);

export const getStatDef = (id: number) =>
	request.get<any, { data: { result: LkCustomStatDef } }>(`${BASE}/detail`, { params: { id } });

export const addStatDef = (data: Omit<LkCustomStatDef, 'id' | 'createTime' | 'updateTime'>) =>
	request.post(`${BASE}/add`, data);

export const updateStatDef = (data: LkCustomStatDef) =>
	request.post(`${BASE}/update`, data);

export const deleteStatDef = (id: number) =>
	request.post(`${BASE}/delete`, { id });

export const runStat = (input: RunStatInput) =>
	request.post<any, { data: { result: LkCustomStatResult } }>(`${BASE}/run`, input);

/** Export — returns blob */
export const exportStat = (input: RunStatInput) =>
	request.post(`${BASE}/export`, input, { responseType: 'blob' });
