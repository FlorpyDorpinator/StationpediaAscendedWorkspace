/**
 * Tests for TooltipItemEditor component
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TooltipItemEditor } from '../TooltipItemEditor';
import type { TooltipDefinition } from '@models/tooltipModel';

describe('TooltipItemEditor', () => {
  const mockTooltip: TooltipDefinition = {
    key: 'ItemBattery',
    description: 'Standard rechargeable power cell',
    category: 'Item',
  };

  it('should render with tooltip data', () => {
    render(<TooltipItemEditor tooltip={mockTooltip} />);

    expect(screen.getByText('ItemBattery')).toBeInTheDocument();
    expect(screen.getByText(/Category:/)).toBeInTheDocument();
  });

  it('should display message when no tooltip is selected', () => {
    render(<TooltipItemEditor tooltip={null} />);
    expect(screen.getByText('Select a tooltip to edit')).toBeInTheDocument();
  });

  it('should populate editor with tooltip description', () => {
    const { container } = render(<TooltipItemEditor tooltip={mockTooltip} />);

    // The RichTextEditor should contain the content
    expect(container.textContent).toContain('Standard rechargeable power cell');
  });

  it('should call onUpdate when Save is clicked', async () => {
    const user = userEvent.setup();
    const onUpdate = vi.fn();

    const { rerender } = render(
      <TooltipItemEditor tooltip={mockTooltip} onUpdate={onUpdate} />
    );

    // Simulate editing
    const saveButton = screen.getByText('Save');
    await user.click(saveButton);

    expect(onUpdate).toHaveBeenCalledWith(expect.objectContaining({ key: 'ItemBattery' }));
  });

  it('should call onCancel when Cancel is clicked', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();

    render(<TooltipItemEditor tooltip={mockTooltip} onCancel={onCancel} />);

    const cancelButton = screen.getByText('Cancel');
    await user.click(cancelButton);

    expect(onCancel).toHaveBeenCalled();
  });

  it('should update when tooltip prop changes', () => {
    const newTooltip: TooltipDefinition = {
      key: 'ItemCanister',
      description: 'Portable gas storage',
      category: 'Item',
    };

    const { rerender } = render(<TooltipItemEditor tooltip={mockTooltip} />);
    expect(screen.getByText('ItemBattery')).toBeInTheDocument();

    rerender(<TooltipItemEditor tooltip={newTooltip} />);
    expect(screen.getByText('ItemCanister')).toBeInTheDocument();
  });

  it('should have editor and preview panels', () => {
    render(<TooltipItemEditor tooltip={mockTooltip} />);

    expect(screen.getByText('Description')).toBeInTheDocument();
    expect(screen.getByText('Preview')).toBeInTheDocument();
  });

  it('should show preview section', () => {
    render(<TooltipItemEditor tooltip={mockTooltip} />);

    // Preview should show the tooltip content
    const previewArea = screen.getByText('Preview').closest('div');
    expect(previewArea).toBeInTheDocument();
  });

  it('should display category if available', () => {
    const tooltipWithCategory: TooltipDefinition = {
      key: 'ItemBattery',
      description: 'Description',
      category: 'Item',
    };

    render(<TooltipItemEditor tooltip={tooltipWithCategory} />);
    expect(screen.getByText('Category: Item')).toBeInTheDocument();
  });

  it('should not display category if not available', () => {
    const tooltipWithoutCategory: TooltipDefinition = {
      key: 'ItemBattery',
      description: 'Description',
    };

    render(<TooltipItemEditor tooltip={tooltipWithoutCategory} />);
    expect(screen.queryByText(/Category:/)).not.toBeInTheDocument();
  });

  it('should have readonly key display', () => {
    render(<TooltipItemEditor tooltip={mockTooltip} />);

    const keyDisplay = screen.getByText('ItemBattery');
    expect(keyDisplay).toBeInTheDocument();

    // Key should be in a non-editable section
    const keySection = screen.getByText('Tooltip Key').closest('div');
    expect(keySection).toBeInTheDocument();
  });

  it('should preserve description when tooltip changes', async () => {
    const { rerender } = render(<TooltipItemEditor tooltip={mockTooltip} />);

    const newTooltip: TooltipDefinition = {
      ...mockTooltip,
      description: 'Updated description',
    };

    rerender(<TooltipItemEditor tooltip={newTooltip} />);

    expect(screen.getByText('Updated description')).toBeInTheDocument();
  });

  it('should reset description on Cancel', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();

    render(<TooltipItemEditor tooltip={mockTooltip} onCancel={onCancel} />);

    const cancelButton = screen.getByText('Cancel');
    await user.click(cancelButton);

    expect(onCancel).toHaveBeenCalled();
  });
});
