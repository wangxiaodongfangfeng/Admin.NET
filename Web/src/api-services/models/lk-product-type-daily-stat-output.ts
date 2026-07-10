/* tslint:disable */
/* eslint-disable */
/**
 * xxx业务应用
 */

/**
 * 单个产品类型的每日检测统计结果
 *
 * @export
 * @interface LkProductTypeDailyStatOutput
 */
export interface LkProductTypeDailyStatOutput {
    /**
     * 产品类型ID
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    productTypeId?: number;

    /**
     * 产品类型名称
     * @type {string}
     * @memberof LkProductTypeDailyStatOutput
     */
    productTypeName?: string | null;

    /**
     * 统计天起始日期，格式 yyyy-MM-dd
     * @type {string}
     * @memberof LkProductTypeDailyStatOutput
     */
    date?: string | null;

    /**
     * 产品状态ID
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    productStatusId?: number;

    /**
     * 产品状态名称
     * @type {string}
     * @memberof LkProductTypeDailyStatOutput
     */
    productStatusName?: string | null;

    /**
     * 检测总数
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    total?: number;

    /**
     * OK 数量
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    okCount?: number;

    /**
     * NG 数量
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    ngCount?: number;

    /**
     * 合格率（保留4位小数，例如 0.9523 表示 95.23%）
     * @type {number}
     * @memberof LkProductTypeDailyStatOutput
     */
    passRate?: number;
}
